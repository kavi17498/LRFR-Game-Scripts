using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI highScore;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Player player;

    [Header("Game Over Screen UI")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;

    private int coinCount = 0;
    private float scoreTimer;
    private int score;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        coinCount = GameManager.Instance.Coins;
        coinText.text = coinCount.ToString();
        highScore.text = GameManager.Instance.HighestScore.ToString();

        score = 0;
        scoreTimer = 0f;
        scoreText.text = "0";

        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        if (player != null)
        {
            player.UpdateSpeedForScore(score);
        }
    }

    private void Update()
    {
        scoreTimer += Time.deltaTime;

        // Increase score every 0.1 second
        if (scoreTimer >= 0.1f)
        {
            score++;
            scoreTimer = 0f;

            scoreText.text = score.ToString();

            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }

            if (player != null)
            {
                player.UpdateSpeedForScore(score);
            }
        }
    }

    public void GameOver()
    {
        // Update high score in GameManager
        GameManager.Instance.UpdateHighestScore(score);

        // Update main gameplay UI high score
        highScore.text = GameManager.Instance.HighestScore.ToString();

        // Update Game Over panel text components
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = score.ToString();
        }

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = GameManager.Instance.HighestScore.ToString();
        }

        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public void addcoins()
    {
        coinCount++;
        GameManager.Instance.AddCoin(1);
        coinText.text = coinCount.ToString();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Welcome");
    }
}