using UnityEngine;

public class InfiniteTile : MonoBehaviour
{
    [SerializeField] private float tileWidth = 20f;
    [SerializeField] private Transform player;

    private void Update()
    {
        if (player.position.x - transform.position.x > tileWidth)
        {
            transform.position += Vector3.right * tileWidth * 2f;
        }
    }
}