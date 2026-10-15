using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUIManager : MonoBehaviour
{
    [Header("Level Buttons")]
    [SerializeField] private LevelButton[] levelButtons;

    [Header("Navigation")]
    [SerializeField] private string homeSceneName = "Welcome";
    [SerializeField] private string gameplaySceneName = "Gameplay";

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        // If not assigned in Inspector, find all buttons in scene
        if (levelButtons == null || levelButtons.Length == 0)
        {
#if UNITY_2023_1_OR_NEWER
            levelButtons = FindObjectsByType<LevelButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            levelButtons = FindObjectsOfType<LevelButton>(true);
#endif
            Array.Sort(levelButtons, (a, b) => a.GetExtractedLevelIndex().CompareTo(b.GetExtractedLevelIndex()));
        }

        foreach (var btn in levelButtons)
        {
            if (btn == null) continue;

            int index = btn.GetExtractedLevelIndex();
            bool isUnlocked = false;
            int stars = 0;

            if (LevelManager.Instance != null)
            {
                var lvlData = LevelManager.Instance.GetLevel(index);
                if (lvlData != null)
                {
                    isUnlocked = lvlData.isUnlocked;
                    stars = lvlData.starsEarned;
                }
                else
                {
                    isUnlocked = (index == 1);
                }
            }
            else
            {
                isUnlocked = (index == 1);
            }

            btn.Setup(index, isUnlocked, stars);
        }
    }

    public void OnLevelSelected(int levelIndex)
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartLevel(levelIndex);
        }
        else
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    public void BackToHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }
}
