using UnityEngine;
using System.Collections.Generic; // Cần cho List

public class ItemSpawner : MonoBehaviour
{
    // A. Các Prefabs Item
    [Header("Item Prefabs")]
    public GameObject[] itemPrefabs; // Mảng chứa 4 Item Prefabs

    // B. Các Vị Trí Xuất Hiện
    [Header("Spawn Points")]
    public Transform[] spawnPoints; // Mảng chứa 8 Vị trí Spawn

    // Số lượng Items muốn xuất hiện (ví dụ: chỉ muốn 3 Items trong 8 chỗ)
    [Header("Spawn Settings")]
    public int numberOfItemsToSpawn = 3; 

    void Start()
    {
        // Khởi động quá trình xuất hiện ngẫu nhiên
        SpawnItemsRandomly();
    }

    void SpawnItemsRandomly()
    {
        // 1. Tạo danh sách các vị trí Spawn đã có sẵn (từ 8 vị trí)
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        
        // Giới hạn số lượng Items cần xuất hiện
        int itemsToSpawn = Mathf.Min(numberOfItemsToSpawn, availableSpawnPoints.Count);

        for (int i = 0; i < itemsToSpawn; i++)
        {
            // 2. Chọn ngẫu nhiên một vị trí Spawn còn trống
            int randomPointIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomPointIndex];

            // 3. Chọn ngẫu nhiên một Item từ mảng 4 Item Prefabs
            int randomItemIndex = Random.Range(0, itemPrefabs.Length);
            GameObject itemToSpawn = itemPrefabs[randomItemIndex];

            // 4. Instantiate Item tại vị trí đã chọn
            Instantiate(itemToSpawn, spawnPoint.position, Quaternion.identity);

            // 5. Loại bỏ vị trí này khỏi danh sách khả dụng để không bị trùng lặp
            availableSpawnPoints.RemoveAt(randomPointIndex);
        }
    }
}