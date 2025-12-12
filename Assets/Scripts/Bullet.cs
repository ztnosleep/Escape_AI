using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float bulletSpeed = 20f;
    public float knockbackForce = 5f; // Lực đánh lùi
    public float lifetime = 3f;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    // Được gọi từ PlayerCombat để thiết lập hướng bắn
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        rb.linearVelocity = direction * bulletSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đạn có trúng Zombie (Enemy) không
        if (other.CompareTag("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            ApplyKnockback(other.gameObject);
        }
        
        // Đạn biến mất sau khi va chạm với vật cản hoặc kẻ thù
        Destroy(gameObject);
    }

    void ApplyKnockback(GameObject enemy)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();

        if (enemyRb != null)
        {
            // Hướng đánh lùi là hướng ngược lại với hướng đạn đang bay
            Vector2 knockbackDirection = -direction; 
            
            // Gán lực đánh lùi
            enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            // TODO: Ở đây bạn có thể gọi hàm trừ máu của Zombie (ví dụ: enemy.GetComponent<ZombieHealth>().TakeDamage(1);)
        }
    }
}