using UnityEngine;

public class BackgroundSpawner : MonoBehaviour
{
    public GameObject backgroundPrefab;

    public int width = 25;
    public int height = 14;
    public float tileSize = 1f;

    public Vector2 startOffset; // ⭐ vị trí bắt đầu

    void Start()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(
                    startOffset.x + x * tileSize,
                    startOffset.y + y * tileSize,
                    10
                );

                Instantiate(backgroundPrefab, pos, Quaternion.identity, transform);
            }
        }
    }
}
