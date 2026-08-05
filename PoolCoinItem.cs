using UnityEngine;

public class PooledCoinItem : MonoBehaviour
{
    private Transform player;
    private float despawnDistance;

    public void Setup(Transform playerTransform, float distance)
    {
        player = playerTransform;
        despawnDistance = distance;
    }

    void Update()
    {
        if (player == null)
            return;

        if (transform.position.x < player.position.x - despawnDistance)
        {
            gameObject.SetActive(false);
        }
    }
}