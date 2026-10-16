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

    private const string CurrentLevelKey = "CurrentLevelIndex";
    private const string HighestUnlockedLevelKey = "HighestUnlockedLevel";

    [Header("Levels Configuration (Defined in Inspector)")]
    public Levels[] levels;

    [Header("Star Criteria Settings (Boot Scene)")]
    [Tooltip("Percentage of spawned coins required for 2 Stars (e.g. 95 for 95%)")]
    [Range(0f, 100f)]
    public float twoStarCoinPercentage = 95f;

    [Tooltip("Percentage of spawned reward objects required for 3 Stars (e.g. 100 for 100%)")]
    [Range(0f, 100f)]
    public float threeStarRewardPercentage = 100f;

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

    public void LoadProgress()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogWarning("[LevelManager] No levels configured in Inspector! Add levels in the LevelManager component.");
            return;
        }

        // Normalize indices in case Inspector items have default 0 or non-sequential indices
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] == null) continue;

            if (levels[i].levelIndex <= 0 || (i > 0 && levels[i].levelIndex <= levels[i - 1].levelIndex))
            {
                levels[i].levelIndex = i + 1;
            }
        }

        // Load current level index from PlayerPrefs
        currentLevelIndex = PlayerPrefs.GetInt(CurrentLevelKey, 1);
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 1, levels.Length);

        // Load progress for each user-defined level
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] == null) continue;

            int idx = levels[i].levelIndex;
            // The first level in the list is always unlocked by default
            int defaultUnlocked = (i == 0) ? 1 : 0;
            levels[i].isUnlocked = PlayerPrefs.GetInt($"Level_{idx}_Unlocked", defaultUnlocked) == 1;
            levels[i].starsEarned = PlayerPrefs.GetInt($"Level_{idx}_Stars", 0);
        }
    }

    public Levels GetLevel(int index)
    {
        if (levels == null || levels.Length == 0) return null;

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null && levels[i].levelIndex == index)
                return levels[i];
        }

        // Fallback: 1-based array index lookup
        int arrayIdx = index - 1;
        if (arrayIdx >= 0 && arrayIdx < levels.Length)
        {
            return levels[arrayIdx];
        }

        return null;
    }

    public void StartLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        PlayerPrefs.SetInt(CurrentLevelKey, currentLevelIndex);
        PlayerPrefs.Save();

        ResetRunStats();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Gameplay");
    }

    public void SetCurrentLevelIndex(int newIndex)
    {
        currentLevelIndex = newIndex;
        PlayerPrefs.SetInt(CurrentLevelKey, currentLevelIndex);
        PlayerPrefs.Save();
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
    /// 1 Star: Complete the mission.
    /// 2 Stars: Complete mission + collect >= 95% of spawned coins.
    /// 3 Stars: Complete mission + >= 95% spawned coins + collect ALL spawned reward objects.
    /// </summary>
    public int CalculateStars()
    {
        int stars = 1; // 1 Star for completing the mission

        // Determine coin percentage for 2 stars (per-level override or global setting)
        float coinPct = twoStarCoinPercentage;
        if (CurrentLevel != null && CurrentLevel.coinPercentageForTwoStars > 0f)
        {
            coinPct = CurrentLevel.coinPercentageForTwoStars;
        }
        float coinRatioRequired = coinPct > 1f ? coinPct / 100f : coinPct;

        // Check 2 Stars condition: Must have spawned coins and collected >= required percentage
        bool coinCondition = TotalCoinsSpawned > 0 &&
                             CoinsCollectedThisRun > 0 &&
                             (((float)CoinsCollectedThisRun / TotalCoinsSpawned) >= coinRatioRequired);

        if (coinCondition)
        {
            stars = 2;

            // Determine reward percentage for 3 stars (per-level override or global setting)
            float rewardPct = threeStarRewardPercentage;
            if (CurrentLevel != null && CurrentLevel.rewardPercentageForThreeStars > 0f)
            {
                rewardPct = CurrentLevel.rewardPercentageForThreeStars;
            }
            float rewardRatioRequired = rewardPct > 1f ? rewardPct / 100f : rewardPct;

            // Check 3 Stars condition: Must have spawned rewards and collected >= required percentage
            bool rewardCondition = TotalRewardsSpawned > 0 &&
                                   RewardsCollectedThisRun > 0 &&
                                   (((float)RewardsCollectedThisRun / TotalRewardsSpawned) >= rewardRatioRequired);

            if (rewardCondition)
            {
                stars = 3;
            }
        }

        float coinPctAchieved = TotalCoinsSpawned > 0 ? ((float)CoinsCollectedThisRun / TotalCoinsSpawned * 100f) : 0f;
        Debug.Log($"[LevelManager] CalculateStars: Coins={CoinsCollectedThisRun}/{TotalCoinsSpawned} ({coinPctAchieved:0}%, req {coinPct:0}%), Rewards={RewardsCollectedThisRun}/{TotalRewardsSpawned} => Awarded {stars} Star(s) this round.");

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

        // Unlock next level if one exists in the Inspector-defined levels
        int nextIndex = levelIndex + 1;
        Levels nextLvl = GetLevel(nextIndex);
        if (nextLvl != null)
        {
            nextLvl.isUnlocked = true;
            PlayerPrefs.SetInt($"Level_{nextIndex}_Unlocked", 1);

            // Update active level index to next level and save
            currentLevelIndex = nextIndex;
            PlayerPrefs.SetInt(CurrentLevelKey, currentLevelIndex);
        }
        else
        {
            // All levels completed
            currentLevelIndex = levelIndex;
            PlayerPrefs.SetInt(CurrentLevelKey, currentLevelIndex);
        }

        int highest = Mathf.Max(PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1), currentLevelIndex);
        PlayerPrefs.SetInt(HighestUnlockedLevelKey, highest);
        PlayerPrefs.Save();

        Debug.Log($"[LevelManager] Level {levelIndex} completed with {stars} stars! CurrentLevelIndex is now: {currentLevelIndex}. Saved in PlayerPrefs.");
    }

    public void ResetAllProgress()
    {
        if (levels != null)
        {
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] == null) continue;
                int idx = levels[i].levelIndex;
                levels[i].isUnlocked = (i == 0);
                levels[i].starsEarned = 0;
                PlayerPrefs.DeleteKey($"Level_{idx}_Unlocked");
                PlayerPrefs.DeleteKey($"Level_{idx}_Stars");
            }
        }
        currentLevelIndex = 1;
        PlayerPrefs.SetInt(CurrentLevelKey, 1);
        PlayerPrefs.SetInt(HighestUnlockedLevelKey, 1);
        PlayerPrefs.Save();
    }
}
