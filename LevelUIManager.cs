using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUIManager : MonoBehaviour
{
    [Header("Level Buttons (Optional: Leave empty to auto-detect from scene)")]
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
        // Ensure LevelManager has loaded latest PlayerPrefs
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadProgress();
        }

        // Find all LevelButton components in the scene
        LevelButton[] sceneButtons;
#if UNITY_2023_1_OR_NEWER
        sceneButtons = FindObjectsByType<LevelButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        sceneButtons = FindObjectsOfType<LevelButton>(true);
#endif
        if (sceneButtons != null && sceneButtons.Length > 0)
        {
            levelButtons = sceneButtons;
            Array.Sort(levelButtons, (a, b) => a.GetExtractedLevelIndex().CompareTo(b.GetExtractedLevelIndex()));
        }

        if (levelButtons == null || levelButtons.Length == 0)
            return;

        int definedLevelCount = 0;
        if (LevelManager.Instance != null && LevelManager.Instance.levels != null)
        {
            definedLevelCount = LevelManager.Instance.levels.Length;
        }

        // If no LevelManager is active (e.g. running Levels scene standalone), try reading from PlayerPrefs
        if (definedLevelCount == 0)
        {
            definedLevelCount = levelButtons.Length;
        }

        // For each button in the scene:
        // If an Inspector-defined level exists for this button, activate and set it up.
        // If no level is defined for this button (e.g., user only added 2 levels), deactivate the button.
        for (int i = 0; i < levelButtons.Length; i++)
        {
            var btn = levelButtons[i];
            if (btn == null) continue;

            if (i < definedLevelCount)
            {
                // This level is defined by the user
                btn.gameObject.SetActive(true);

                int levelIndex = i + 1;
                bool isUnlocked = (i == 0);
                int stars = 0;

                if (LevelManager.Instance != null)
                {
                    var lvlData = LevelManager.Instance.GetLevel(levelIndex);
                    if (lvlData != null)
                    {
                        levelIndex = lvlData.levelIndex;
                        isUnlocked = lvlData.isUnlocked;
                        stars = lvlData.starsEarned;
                    }
                }
                else
                {
                    isUnlocked = PlayerPrefs.GetInt($"Level_{levelIndex}_Unlocked", (i == 0 ? 1 : 0)) == 1;
                    stars = PlayerPrefs.GetInt($"Level_{levelIndex}_Stars", 0);
                }

                btn.Setup(levelIndex, isUnlocked, stars);
            }
            else
            {
                // Button exceeds the number of levels defined in the Inspector: hide it
                btn.gameObject.SetActive(false);
            }
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
            PlayerPrefs.SetInt("CurrentLevelIndex", levelIndex);
            PlayerPrefs.Save();
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    public void BackToHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }
}
