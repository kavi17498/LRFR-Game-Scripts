using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;

    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI highScore;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private Player player;

    [Header("Game Over Screen UI")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;

    [Header("Level Complete Screen UI")]
    [SerializeField] private TextMeshProUGUI levelCompleteTitleText;
    [SerializeField] private TextMeshProUGUI levelCompleteMissionText;
    [SerializeField] private TextMeshProUGUI levelCompleteStarsText;
    [SerializeField] private TextMeshProUGUI levelCompleteCoinsRatioText;
    [SerializeField] private TextMeshProUGUI levelCompleteRewardsRatioText;
    [SerializeField] private Image levelCompleteStarImage;
    [SerializeField] private Sprite oneStarSprite;
    [SerializeField] private Sprite twoStarSprite;
    [SerializeField] private Sprite threeStarSprite;

    private int coinCount = 0;
    private int coinsCollectedThisRun = 0;
    private float scoreTimer;
    private float runTimer;
    private int score;
    private bool isGameOver = false;
    private bool isLevelComplete = false;
    private int activeLevelIndex = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetRunStats();
        }
    }

    private void Start()
    {
        coinCount = GameManager.Instance != null ? GameManager.Instance.Coins : 0;
        if (coinText != null) coinText.text = coinCount.ToString();
        if (highScore != null && GameManager.Instance != null) highScore.text = GameManager.Instance.HighestScore.ToString();

        score = 0;
        scoreTimer = 0f;
        runTimer = 0f;
        coinsCollectedThisRun = 0;
        isGameOver = false;
        isLevelComplete = false;

        if (scoreText != null) scoreText.text = "0";

        if (player == null)
        {
#if UNITY_2023_1_OR_NEWER
            player = FindAnyObjectByType<Player>();
#else
            player = FindObjectOfType<Player>();
#endif
        }

        if (player != null)
        {
            player.UpdateSpeedForScore(score);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetRunStats();
            activeLevelIndex = LevelManager.Instance.currentLevelIndex;
            UpdateMissionUI();
            Debug.Log($"[GamePlayManager] Playing Level {activeLevelIndex}: {LevelManager.Instance.CurrentLevel?.GetMissionDescription()}");
        }
        else
        {
            activeLevelIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 1);
        }
    }

    private void Update()
    {
        if (isGameOver || isLevelComplete)
            return;

        runTimer += Time.deltaTime;
        scoreTimer += Time.deltaTime;

        // Increase score every 0.1 second
        if (scoreTimer >= 0.1f)
        {
            score++;
            scoreTimer = 0f;

            if (scoreText != null) scoreText.text = score.ToString();

            if (player == null)
            {
#if UNITY_2023_1_OR_NEWER
                player = FindAnyObjectByType<Player>();
#else
                player = FindObjectOfType<Player>();
#endif
            }

            if (player != null)
            {
                player.UpdateSpeedForScore(score);
            }
        }

        // Check if level mission objective is accomplished
        if (LevelManager.Instance != null)
        {
            if (LevelManager.Instance.CheckMissionGoal(runTimer, score, coinsCollectedThisRun))
            {
                LevelComplete();
            }
        }
    }

    private void UpdateMissionUI()
    {
        if (missionText != null && LevelManager.Instance != null)
        {
            var currentLvl = LevelManager.Instance.CurrentLevel;
            if (currentLvl != null)
            {
                missionText.text = $"Level {currentLvl.levelIndex}: {currentLvl.GetMissionDescription()}";
            }
        }
    }

    public void GameOver()
    {
        if (isGameOver || isLevelComplete) return;

        isGameOver = true;

        // Update high score in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHighestScore(score);
            if (highScore != null) highScore.text = GameManager.Instance.HighestScore.ToString();
        }

        // Update Game Over panel text components
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = score.ToString();
        }

        if (gameOverHighScoreText != null && GameManager.Instance != null)
        {
            gameOverHighScoreText.text = GameManager.Instance.HighestScore.ToString();
        }

        Time.timeScale = 0f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void LevelComplete()
    {
        if (isLevelComplete || isGameOver) return;

        isLevelComplete = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHighestScore(score);
        }

        int completedLevel = activeLevelIndex;
        int stars = 1;
        int nextLevel = activeLevelIndex + 1;

        if (LevelManager.Instance != null)
        {
            completedLevel = LevelManager.Instance.currentLevelIndex;
            stars = LevelManager.Instance.CalculateStars();
            LevelManager.Instance.CompleteLevel(completedLevel, stars);
            nextLevel = LevelManager.Instance.currentLevelIndex;
        }

        Time.timeScale = 0f;

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            if (levelCompleteTitleText != null)
            {
                levelCompleteTitleText.text = $"Level {completedLevel} Complete!";
            }

            if (levelCompleteMissionText != null && LevelManager.Instance != null)
            {
                var lvl = LevelManager.Instance.GetLevel(completedLevel);
                if (lvl != null)
                    levelCompleteMissionText.text = lvl.GetMissionDescription();
            }

            if (levelCompleteStarsText != null)
            {
                levelCompleteStarsText.text = $"{stars} / 3 Stars Earned";
            }

            if (levelCompleteStarImage != null)
            {
                switch (stars)
                {
                    case 1:
                        if (oneStarSprite != null) levelCompleteStarImage.sprite = oneStarSprite;
                        break;
                    case 2:
                        if (twoStarSprite != null) levelCompleteStarImage.sprite = twoStarSprite;
                        break;
                    case 3:
                        if (threeStarSprite != null) levelCompleteStarImage.sprite = threeStarSprite;
                        break;
                }
            }

            if (levelCompleteCoinsRatioText != null && LevelManager.Instance != null)
            {
                int totalCoins = LevelManager.Instance.TotalCoinsSpawned;
                int collectedCoins = LevelManager.Instance.CoinsCollectedThisRun;
                float pct = totalCoins > 0 ? ((float)collectedCoins / totalCoins * 100f) : 100f;
                levelCompleteCoinsRatioText.text = $"Coins: {collectedCoins}/{totalCoins} ({pct:0}%)";
            }

            if (levelCompleteRewardsRatioText != null && LevelManager.Instance != null)
            {
                int totalRewards = LevelManager.Instance.TotalRewardsSpawned;
                int collectedRewards = LevelManager.Instance.RewardsCollectedThisRun;
                levelCompleteRewardsRatioText.text = $"Rewards: {collectedRewards}/{totalRewards}";
            }
        }
        else
        {
            // Fallback if dedicated panel is not set up in scene:
            // show gameOverPanel updated with Level Complete info and next level
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                if (gameOverScoreText != null)
                {
                    gameOverScoreText.text = $"LEVEL {completedLevel} COMPLETE!\nStars: {stars}/3\nNext: Level {nextLevel}";
                }
            }
        }
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartLevel(LevelManager.Instance.currentLevelIndex);
            return;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToLevels()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Levels");
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void addcoins()
    {
        coinCount++;
        coinsCollectedThisRun++;
        if (GameManager.Instance != null) GameManager.Instance.AddCoin(1);
        if (coinText != null) coinText.text = coinCount.ToString();
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