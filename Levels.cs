using System;
using UnityEngine;

public enum MissionType
{
    SurviveTime,    // Run/survive for targetGoal seconds
    ReachScore,     // Reach targetGoal score
    CollectCoins    // Collect targetGoal coins in this run
}

[System.Serializable]
public class Levels
{
    public string levelName;
    public int levelIndex;
    public MissionType missionType;
    public float targetGoal;      // e.g. 30 (seconds), 1000 (score), 100 (coins)
    public int starsEarned;       // 0 to 3
    public bool isUnlocked;

    [Header("Star Criteria Overrides (0 = use LevelManager global %)")]
    [Range(0f, 100f)]
    public float coinPercentageForTwoStars = 0f;
    [Range(0f, 100f)]
    public float rewardPercentageForThreeStars = 0f;

    public string GetMissionDescription()
    {
        switch (missionType)
        {
            case MissionType.SurviveTime:
                return $"Survive for {targetGoal:0} seconds";
            case MissionType.ReachScore:
                return $"Reach a score of {targetGoal:0}";
            case MissionType.CollectCoins:
                return $"Collect {targetGoal:0} coins";
            default:
                return "Complete the run";
        }
    }
}