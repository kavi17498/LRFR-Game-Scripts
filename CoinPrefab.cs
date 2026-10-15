using UnityEngine;

public class CoinPrefab : MonoBehaviour
{
    [SerializeField] private float magnetSpeed = 12f;

    private bool isMagnetized = false;
    private Transform magnetTarget;

    private void Update()
    {
        if (!isMagnetized || magnetTarget == null)
            return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            magnetTarget.position,
            magnetSpeed * Time.deltaTime);
    }

    public void StartMagnet(Transform target)
    {
        isMagnetized = true;
        magnetTarget = target;
    }

    private void OnEnable()
    {
        // Reset when reused from the object pool
        isMagnetized = false;
        magnetTarget = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Coin collected!");
        GamePlayManager.Instance.addcoins();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnCoinCollected();
        }

        // Reset before returning to pool
        isMagnetized = false;
        magnetTarget = null;

        gameObject.SetActive(false);
    }
}