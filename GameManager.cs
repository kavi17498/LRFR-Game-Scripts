using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int HighestScore { get; private set; }
    public int Coins { get; private set; }

    private const string HighestScoreKey = "HighestScore";
    private const string CoinsKey = "Coins";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
    }

    private void LoadData()
    {
        HighestScore = PlayerPrefs.GetInt(HighestScoreKey, 0);
        Coins = PlayerPrefs.GetInt(CoinsKey, 0);
    }

    public void AddCoin(int amount = 1)
    {
        Coins += amount;
        SaveCoins();
    }

    public bool SpendCoins(int amount)
    {
        if (Coins < amount)
            return false;

        Coins -= amount;
        SaveCoins();

        return true;
    }

    public void UpdateHighestScore(int score)
    {
        if (score > HighestScore)
        {
            HighestScore = score;
            PlayerPrefs.SetInt(HighestScoreKey, HighestScore);
            PlayerPrefs.Save();
        }
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinsKey, Coins);
        PlayerPrefs.Save();
    }
}