using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Cài đặt Giá trị")]
    public int coinValue = 10; // Mặc định 1 đồng = 10 coins

    [Header("Hiệu ứng")]
    public AudioClip pickupSound; // Tiếng "Ting ting" khi nhặt

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem cái chạm vào có phải là Player không
        if (other.CompareTag("Player"))
        {
            // 1. Cộng tiền vào GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin(coinValue);
                
                // Lưu lại ngay lập tức (để lỡ chết vẫn có tiền)
                GameManager.Instance.SaveData(); 
                
                Debug.Log("Nhặt được " + coinValue + " coins!");
            }

            // 2. Phát âm thanh (nếu có)
            // Dùng PlayClipAtPoint để âm thanh vẫn kêu sau khi đồng tiền bị hủy
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // 3. Hủy đồng vàng đi (biến mất)
            Destroy(gameObject);
        }
    }
}