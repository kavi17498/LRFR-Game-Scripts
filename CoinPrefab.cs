using UnityEngine;

public class CoinPrefab : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Add coin/score here
        // GameManager.Instance.AddCoin();
        Debug.Log("Coin collected!");
        GamePlayManager.Instance.addcoins();
        gameObject.SetActive(false);
    }
}