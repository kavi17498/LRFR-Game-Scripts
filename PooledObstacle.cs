using UnityEngine;

public class PooledObstacle : MonoBehaviour
{
    public float yOffset = 0f;
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

        if (transform.position.x <
            player.position.x - despawnDistance)
        {
            gameObject.SetActive(false);
        }
    }
}