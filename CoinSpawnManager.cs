using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform player;

    [Header("Pool")]
    [SerializeField] private int poolSize = 30;

    [Header("Coin Trail")]
    [SerializeField] private float coinSpacing = 0.6f;
    [SerializeField] private float timeBetweenCoins = 0.08f;

    [SerializeField] private int minCoinsPerTrail = 3;
    [SerializeField] private int maxCoinsPerTrail = 8;

    [Header("Height")]
    [SerializeField] private float minY = -1f;
    [SerializeField] private float maxY = 3f;

    [Header("Delay Between Trails")]
    [SerializeField] private float minTrailDelay = 1f;
    [SerializeField] private float maxTrailDelay = 2f;

    [SerializeField] private float despawnDistance = 20f;

    private List<GameObject> pool = new();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false);
            pool.Add(coin);
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float y = Random.Range(minY, maxY);
            int coinCount = Random.Range(minCoinsPerTrail, maxCoinsPerTrail + 1);

            for (int i = 0; i < coinCount; i++)
            {
                SpawnCoin(i * coinSpacing, y);
                yield return new WaitForSeconds(timeBetweenCoins);
            }

            yield return new WaitForSeconds(
                Random.Range(minTrailDelay, maxTrailDelay));
        }
    }

    void SpawnCoin(float xOffset, float y)
    {
        foreach (GameObject coin in pool)
        {
            if (!coin.activeInHierarchy)
            {
                coin.transform.position = new Vector3(
                    spawnPoint.position.x + xOffset,
                    y,
                    spawnPoint.position.z);

                coin.SetActive(true);

                coin.GetComponent<PooledCoinItem>()
                    .Setup(player, despawnDistance);

                return;
            }
        }
    }
}