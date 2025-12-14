using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Cài đặt Vật phẩm")]
    public GameObject coinPrefab;   // Kéo Prefab đồng tiền vào đây
    public int totalCoins = 10;     // Số lượng tiền muốn sinh ra

    [Header("Cài đặt Khu vực")]
    public Collider2D spawnArea;    // Kéo BoxCollider2D xác định vùng chơi vào đây
    
    [Header("Tránh vật cản (Tường)")]
    public LayerMask obstacleLayer; // Chọn Layer là "Wall" hoặc "Obstacle"
    public float checkRadius = 0.5f; // Bán kính kiểm tra va chạm (nên to hơn đồng tiền xíu)

    void Start()
    {
        SpawnCoins();
    }

    void SpawnCoins()
    {
        if (spawnArea == null)
        {
            Debug.LogError("Chưa kéo Collider vùng Spawn vào!");
            return;
        }

        int coinsSpawned = 0;
        int attempts = 0; // Biến đếm số lần thử để tránh treo máy nếu map quá chật

        // Vòng lặp chạy cho đến khi sinh đủ số tiền
        while (coinsSpawned < totalCoins && attempts < 1000)
        {
            attempts++;

            // 1. Lấy một điểm ngẫu nhiên trong vùng Collider
            Vector2 randomPos = GetRandomPointInCollider(spawnArea);

            // 2. Kiểm tra xem điểm đó có bị dính tường không?
            // Physics2D.OverlapCircle trả về true nếu có vật cản trong bán kính
            if (!Physics2D.OverlapCircle(randomPos, checkRadius, obstacleLayer))
            {
                // 3. Nếu an toàn (không dính tường) -> Sinh tiền
                Instantiate(coinPrefab, randomPos, Quaternion.identity);
                coinsSpawned++;
            }
        }

        if (coinsSpawned < totalCoins)
        {
            Debug.LogWarning("Chỉ sinh được " + coinsSpawned + " đồng (Map quá chật hoặc nhiều vật cản)");
        }
    }

    // Hàm tiện ích lấy điểm ngẫu nhiên trong Bounds của Collider
    Vector2 GetRandomPointInCollider(Collider2D collider)
    {
        Bounds bounds = collider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector2(x, y);
    }

    // Vẽ hình tròn đỏ trong Editor để dễ hình dung kích thước check vật cản
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}