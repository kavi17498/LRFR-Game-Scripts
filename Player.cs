using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float speedIncreasePerScore = 0.012f;
    [SerializeField] private float speedChangeRate = 0.01f;
    [SerializeField] private float maxMoveSpeed = 7f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Jump Forward Boost")]
    [SerializeField] private float jumpMoveSpeed = 7f;
    [SerializeField] private float boostDuration = 0.2f;

    [Header("Swipe Detection")]
    [SerializeField] private float minSwipeDistance = 50f;

    private Rigidbody2D rb;
    private Animator anim;

    private bool isGrounded = true;

    private Vector2 startTouchPosition;
    private bool isSwiping = false;

    private float currentMoveSpeed;
    private float boostTimer;
    private float baseMoveSpeed;
    private float targetMoveSpeed;

    private bool isGameOver = false;

    [SerializeField] private float magnetRadius = 5f;
    [SerializeField] private float magnetDuration = 5f;
    [SerializeField] private float magnetScanInterval = 0.15f;

    private bool magnetActive;
    private float magnetTimer;
    private float scanTimer;

    [Header("Shield")]
    [SerializeField] private float shieldDuration = 30f;
    [SerializeField, Range(0f, 1f)] private float shieldOpacity = 0.5f;

    private bool shieldActive;
    private float shieldTimer;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        baseMoveSpeed = moveSpeed;
        targetMoveSpeed = baseMoveSpeed;
        currentMoveSpeed = targetMoveSpeed;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void FixedUpdate()
    {
        // Handle temporary speed boost
        if (boostTimer > 0)
        {
            boostTimer -= Time.fixedDeltaTime;
            float boostedTarget = Mathf.Max(targetMoveSpeed, jumpMoveSpeed);
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, boostedTarget, speedChangeRate);
        }
        else
        {
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetMoveSpeed, speedChangeRate);
        }

        // Always move forward
        rb.linearVelocity = new Vector2(currentMoveSpeed, rb.linearVelocity.y);
    }

    public void UpdateSpeedForScore(int score)
    {
        targetMoveSpeed = Mathf.Min(baseMoveSpeed + (score * speedIncreasePerScore), maxMoveSpeed);

        if (boostTimer <= 0f)
        {
            currentMoveSpeed = targetMoveSpeed;
        }
    }

    private void Update()
    {


#if UNITY_EDITOR

       

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TriggerGoDown();
        }

        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            AnalyzeGesture(startTouchPosition, Input.mousePosition);
            isSwiping = false;
        }

        if (magnetActive)
        {
            magnetTimer -= Time.deltaTime;
            scanTimer -= Time.deltaTime;

            if (scanTimer <= 0f)
            {
                ScanForCoins();
                scanTimer = magnetScanInterval;
            }

            if (magnetTimer <= 0f)
            {
                magnetActive = false;
            }
        }
        if (shieldActive)
        {
            shieldTimer -= Time.deltaTime;

            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }

#else

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Ended && isSwiping)
            {
                AnalyzeGesture(startTouchPosition, touch.position);
                isSwiping = false;
            }
        }

#endif
    }
    void ScanForCoins()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            magnetRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Coin"))
            {
                CoinPrefab coin = hit.GetComponent<CoinPrefab>();

                if (coin != null)
                {
                    coin.StartMagnet(transform);
                }
            }
        }
    }

    private void AnalyzeGesture(Vector2 start, Vector2 end)
    {
        float verticalDistance = end.y - start.y;
        float horizontalDistance = end.x - start.x;

        if (Mathf.Abs(verticalDistance) > minSwipeDistance &&
            Mathf.Abs(verticalDistance) > Mathf.Abs(horizontalDistance))
        {
            if (verticalDistance < 0)
            {
                TriggerGoDown();
            }
            else if (verticalDistance > 0 && isGrounded)
            {
                Jump();
            }
        }
        else
        {
            if (isGrounded)
            {
                Jump();
            }
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(currentMoveSpeed, jumpForce);

        // Give a temporary forward speed boost
        currentMoveSpeed = jumpMoveSpeed;
        boostTimer = boostDuration;

        isGrounded = false;
    }

    private void TriggerGoDown()
    {
        anim.SetTrigger("godown");

        // Optional fast fall:
        // if (!isGrounded)
        //     rb.linearVelocity = new Vector2(currentMoveSpeed, -jumpForce);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("Player is grounded.");
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (shieldActive)
            {
                return;
            }
            Debug.Log("Player hit an obstacle!");
            GameOver();
        }

        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle2"))
        {
            // Need to check if the player is in down animation state
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("creep"))
            {
                Debug.Log("Player is in down animation state and hit Obstacle2, no game over.");
            }
            else
            {
                if (shieldActive)
                {
                    return;
                }
                Debug.Log("Player hit Obstacle2!");
                GameOver();
            }
        }
        if (collision.CompareTag("Magnet"))
        {
            magnetActive = true;
            magnetTimer = magnetDuration;

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnRewardCollected();
            }

            collision.gameObject.SetActive(false);
        }
        if (collision.CompareTag("Shield"))
        {
            ActivateShield();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnRewardCollected();
            }

            collision.gameObject.SetActive(false);
        }
    }
    private void ActivateShield()
    {
        shieldActive = true;
        shieldTimer = shieldDuration;

        Color color = spriteRenderer.color;
        color.a = shieldOpacity;
        spriteRenderer.color = color;
    }
    private void DeactivateShield()
    {
        shieldActive = false;
        shieldTimer = 0f;

        spriteRenderer.color = originalColor;
    }

    private void GameOver()
    {
        if (isGameOver)
            return;
        anim.SetTrigger("ishit");
        isGameOver = true;

        Debug.Log("Game Over!");

        

        currentMoveSpeed = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        GamePlayManager.Instance.GameOver();


    }
}