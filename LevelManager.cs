using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
#if UNITY_2023_1_OR_NEWER
                _instance = FindAnyObjectByType<LevelManager>();
#else
                _instance = FindObjectOfType<LevelManager>();
#endif
                if (_instance == null)
                {
                    GameObject obj = new GameObject("LevelManager");
                    _instance = obj.AddComponent<LevelManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    [Header("Levels Configuration")]
    public Levels[] levels;

    [Header("Current Level")]
    public int currentLevelIndex = 1;

    // Run tracking stats
    public int TotalCoinsSpawned { get; private set; }
    public int CoinsCollectedThisRun { get; private set; }
    public int TotalRewardsSpawned { get; private set; }
    public int RewardsCollectedThisRun { get; private set; }
    public bool IsLevelComplete { get; private set; }

    public Levels CurrentLevel => GetLevel(currentLevelIndex);

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProgress();
    }

    private void Start()
    {
        // Ensure default levels exist if not configured
        if (levels == null || levels.Length == 0)
        {
            InitializeDefaultLevels();
            LoadProgress();
        }
    }

    public void InitializeDefaultLevels()
    {
        levels = new Levels[]
        {
            new Levels { levelName = "Level 1", levelIndex = 1, missionType = MissionType.SurviveTime, targetGoal = 30f, isUnlocked = true, starsEarned = 0 },
            new Levels { levelName = "Level 2", levelIndex = 2, missionType = MissionType.ReachScore, targetGoal = 1000f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 3", levelIndex = 3, missionType = MissionType.CollectCoins, targetGoal = 100f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 4", levelIndex = 4, missionType = MissionType.SurviveTime, targetGoal = 45f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 5", levelIndex = 5, missionType = MissionType.ReachScore, targetGoal = 2000f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 6", levelIndex = 6, missionType = MissionType.CollectCoins, targetGoal = 150f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 7", levelIndex = 7, missionType = MissionType.SurviveTime, targetGoal = 60f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 8", levelIndex = 8, missionType = MissionType.ReachScore, targetGoal = 3000f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 9", levelIndex = 9, missionType = MissionType.CollectCoins, targetGoal = 200f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 10", levelIndex = 10, missionType = MissionType.SurviveTime, targetGoal = 75f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 11", levelIndex = 11, missionType = MissionType.ReachScore, targetGoal = 4000f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 12", levelIndex = 12, missionType = MissionType.CollectCoins, targetGoal = 250f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 13", levelIndex = 13, missionType = MissionType.SurviveTime, targetGoal = 90f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 14", levelIndex = 14, missionType = MissionType.ReachScore, targetGoal = 5000f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 15", levelIndex = 15, missionType = MissionType.CollectCoins, targetGoal = 300f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 16", levelIndex = 16, missionType = MissionType.SurviveTime, targetGoal = 105f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 17", levelIndex = 17, missionType = MissionType.ReachScore, targetGoal = 6000f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 18", levelIndex = 18, missionType = MissionType.CollectCoins, targetGoal = 350f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 19", levelIndex = 19, missionType = MissionType.SurviveTime, targetGoal = 120f, isUnlocked = false, starsEarned = 0 },
            new Levels { levelName = "Level 20", levelIndex = 20, missionType = MissionType.ReachScore, targetGoal = 8000f, isUnlocked = false, starsEarned = 0 }
        };
    }

    public void LoadProgress()
    {
        if (levels == null || levels.Length == 0)
        {
            InitializeDefaultLevels();
        }

        for (int i = 0; i < levels.Length; i++)
        {
            int idx = levels[i].levelIndex;
            // Level 1 is always unlocked by default
            int defaultUnlocked = (idx == 1) ? 1 : 0;
            levels[i].isUnlocked = PlayerPrefs.GetInt($"Level_{idx}_Unlocked", defaultUnlocked) == 1;
            levels[i].starsEarned = PlayerPrefs.GetInt($"Level_{idx}_Stars", 0);
        }
    }

    public Levels GetLevel(int index)
    {
        if (levels == null) return null;
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].levelIndex == index)
                return levels[i];
        }
        return null;
    }

    public void StartLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        ResetRunStats();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Gameplay");
    }

    public void ResetRunStats()
    {
        TotalCoinsSpawned = 0;
        CoinsCollectedThisRun = 0;
        TotalRewardsSpawned = 0;
        RewardsCollectedThisRun = 0;
        IsLevelComplete = false;
    }

    // Callbacks from gameplay events
    public void OnCoinSpawned()
    {
        TotalCoinsSpawned++;
    }

    public void OnCoinCollected()
    {
        CoinsCollectedThisRun++;
    }

    public void OnRewardSpawned()
    {
        TotalRewardsSpawned++;
    }

    public void OnRewardCollected()
    {
        RewardsCollectedThisRun++;
    }

    public bool CheckMissionGoal(float runTime, int currentScore, int coinsCollected)
    {
        Levels current = CurrentLevel;
        if (current == null) return false;

        switch (current.missionType)
        {
            case MissionType.SurviveTime:
                return runTime >= current.targetGoal;

            case MissionType.ReachScore:
                return currentScore >= current.targetGoal;

            case MissionType.CollectCoins:
                return coinsCollected >= current.targetGoal;

            default:
                return false;
        }
    }

    /// <summary>
    /// Calculates stars according to rules:
    /// 1 Star: Just complete the mission.
    /// 2 Stars: Complete mission + collect >= 95% of spawned coins.
    /// 3 Stars: Complete mission + >= 95% spawned coins + collect ALL spawned reward objects.
    /// </summary>
    public int CalculateStars()
    {
        int stars = 1;

        // 95% spawned coins condition
        bool coinCondition = (TotalCoinsSpawned == 0) ||
                             (((float)CoinsCollectedThisRun / TotalCoinsSpawned) >= 0.95f);

        if (coinCondition)
        {
            stars = 2;

            // Collect all spawned reward objects condition
            bool rewardCondition = (TotalRewardsSpawned == 0) ||
                                   (RewardsCollectedThisRun >= TotalRewardsSpawned);

            if (rewardCondition)
            {
                stars = 3;
            }
        }

        return stars;
    }

    public void CompleteLevel(int levelIndex, int stars)
    {
        IsLevelComplete = true;

        Levels lvl = GetLevel(levelIndex);
        if (lvl != null)
        {
            if (stars > lvl.starsEarned)
            {
                lvl.starsEarned = stars;
                PlayerPrefs.SetInt($"Level_{levelIndex}_Stars", stars);
            }
        }

        // Unlock next level
        int nextIndex = levelIndex + 1;
        Levels nextLvl = GetLevel(nextIndex);
        if (nextLvl != null)
        {
            nextLvl.isUnlocked = true;
            PlayerPrefs.SetInt($"Level_{nextIndex}_Unlocked", 1);
        }

        PlayerPrefs.Save();
    }

    public void ResetAllProgress()
    {
        if (levels != null)
        {
            for (int i = 0; i < levels.Length; i++)
            {
                int idx = levels[i].levelIndex;
                levels[i].isUnlocked = (idx == 1);
                levels[i].starsEarned = 0;
                PlayerPrefs.DeleteKey($"Level_{idx}_Unlocked");
                PlayerPrefs.DeleteKey($"Level_{idx}_Stars");
            }
            PlayerPrefs.Save();
        }
    }
}
