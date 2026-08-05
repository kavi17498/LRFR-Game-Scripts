using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Reward Prefabs")]
    [SerializeField] private GameObject[] rewardPrefabs;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private Transform player;

    [SerializeField] private float minSpawnTime = 1.2f;
    [SerializeField] private float maxSpawnTime = 2.5f;

    [SerializeField] private float despawnDistance = 20f;

    [SerializeField] private int poolPerPrefab = 6;

    private List<GameObject> pool = new List<GameObject>();
    private List<GameObject> rewardPool = new List<GameObject>();

    void Start()
    {
        CreatePool();
        StartCoroutine(SpawnRoutine());
    }

    void CreatePool()
    {
        foreach (GameObject prefab in obstaclePrefabs)
        {
            for (int i = 0; i < poolPerPrefab; i++)
            {
                GameObject obj = Instantiate(prefab);

                obj.SetActive(false);

                pool.Add(obj);
            }
        }
        foreach (GameObject prefab in rewardPrefabs)
        {
            for (int i = 0; i < poolPerPrefab; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                rewardPool.Add(obj);
            }
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (Random.value < 0.80f)
            {
                SpawnObstacle();
            }
            else
            {
                SpawnReward();
            }

            yield return new WaitForSeconds(
                Random.Range(minSpawnTime, maxSpawnTime));
        }
    }

    void SpawnObstacle()
    {
        List<GameObject> available = new List<GameObject>();

        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                available.Add(obj);
            }
        }

        if (available.Count == 0) return;

        GameObject obstacle = available[Random.Range(0, available.Count)];

        // 1. Get the offset from the obstacle (default to 0 if no script found)
        float yOffset = 0f;
        PooledObstacle data = obstacle.GetComponent<PooledObstacle>();
        if (data != null)
        {
            yOffset = data.yOffset;
        }

        // 2. Calculate the spawn position with the offset
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.y += yOffset;

        // 3. Apply the calculated position
        obstacle.transform.position = spawnPos;
        obstacle.transform.rotation = Quaternion.identity;

        obstacle.SetActive(true);

        obstacle.GetComponent<PooledObstacle>().Setup(player, despawnDistance);
    }

    void SpawnReward()
    {
        List<GameObject> available = new List<GameObject>();

        foreach (GameObject obj in rewardPool)
        {
            if (!obj.activeInHierarchy)
            {
                available.Add(obj);
            }
        }

        if (available.Count == 0) return;

        GameObject obstacle = available[Random.Range(0, available.Count)];

        // 1. Get the offset from the obstacle (default to 0 if no script found)
        float yOffset = 0f;
        PooledObstacle data = obstacle.GetComponent<PooledObstacle>();
        if (data != null)
        {
            yOffset = data.yOffset;
        }

        // 2. Calculate the spawn position with the offset
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.y += yOffset;

        // 3. Apply the calculated position
        obstacle.transform.position = spawnPos;
        obstacle.transform.rotation = Quaternion.identity;

        obstacle.SetActive(true);

        obstacle.GetComponent<PooledObstacle>().Setup(player, despawnDistance);
    }
}