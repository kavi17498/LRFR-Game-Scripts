using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("UI Components")]
    public Button levelButton;

    [Header("Star Sprites")]
    public Sprite YelloImg;        // 0 stars / default background
    public Sprite OneStarSprite;   // 1 star background
    public Sprite TwoStarSprite;   // 2 stars background
    public Sprite ThreeStarSprite; // 3 stars background

    [Header("Level Info")]
    public int levelIndex;

    [Header("Optional Elements")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Sprite lockedSprite;

    private bool isSetup = false;

    private void Awake()
    {
        if (levelButton == null)
            levelButton = GetComponent<Button>();

        if (levelText == null)
            levelText = GetComponentInChildren<TextMeshProUGUI>();

        // Auto-extract index from GameObject name if unassigned or default (e.g. "Level01" -> 1)
        if (levelIndex <= 1)
        {
            int extracted = GetExtractedLevelIndex();
            if (extracted > 0)
            {
                levelIndex = extracted;
            }
        }
    }

    private void Start()
    {
        // Fallback self-init if LevelUIManager hasn't initialized this button yet
        if (!isSetup)
        {
            int idx = GetExtractedLevelIndex();
            bool unlocked = false;
            int stars = 0;

            if (LevelManager.Instance != null)
            {
                var lvl = LevelManager.Instance.GetLevel(idx);
                if (lvl != null)
                {
                    unlocked = lvl.isUnlocked;
                    stars = lvl.starsEarned;
                }
                else
                {
                    unlocked = (idx == 1);
                }
            }
            else
            {
                unlocked = PlayerPrefs.GetInt($"Level_{idx}_Unlocked", (idx == 1 ? 1 : 0)) == 1;
                stars = PlayerPrefs.GetInt($"Level_{idx}_Stars", 0);
            }

            Setup(idx, unlocked, stars);
        }
    }

    public int GetExtractedLevelIndex()
    {
        Match match = Regex.Match(gameObject.name, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int idx))
        {
            return idx;
        }
        return levelIndex > 0 ? levelIndex : 1;
    }

    public void Setup(int index, bool isUnlocked, int starsEarned)
    {
        isSetup = true;
        levelIndex = index;

        if (levelButton == null)
            levelButton = GetComponent<Button>();

        if (levelText == null)
            levelText = GetComponentInChildren<TextMeshProUGUI>();

        if (levelText != null)
        {
            levelText.text = index.ToString();
        }

        if (!isUnlocked)
        {
            levelButton.interactable = false;

            if (lockedSprite != null)
            {
                levelButton.image.sprite = lockedSprite;
                levelButton.image.color = Color.white;
            }
            else if (YelloImg != null)
            {
                levelButton.image.sprite = YelloImg;
                // Dim button slightly if locked and no dedicated locked sprite
                levelButton.image.color = new Color(0.55f, 0.55f, 0.55f, 0.7f);
            }

            if (levelText != null)
            {
                levelText.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
            }
        }
        else
        {
            levelButton.interactable = true;
            levelButton.image.color = Color.white;

            if (levelText != null)
            {
                levelText.color = Color.white;
            }

            switch (starsEarned)
            {
                case 1:
                    if (OneStarSprite != null) levelButton.image.sprite = OneStarSprite;
                    break;
                case 2:
                    if (TwoStarSprite != null) levelButton.image.sprite = TwoStarSprite;
                    break;
                case 3:
                    if (ThreeStarSprite != null) levelButton.image.sprite = ThreeStarSprite;
                    break;
                default:
                    if (YelloImg != null) levelButton.image.sprite = YelloImg;
                    break;
            }
        }

        levelButton.onClick.RemoveAllListeners();
        levelButton.onClick.AddListener(OnButtonClicked);
    }

    public void OnButtonClicked()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartLevel(levelIndex);
        }
        else
        {
            PlayerPrefs.SetInt("CurrentLevelIndex", levelIndex);
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
        }
    }

    public void changebg()
    {
        // If triggered by Inspector persistent OnClick event, start the selected level
        OnButtonClicked();
    }
}
