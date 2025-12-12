using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public Slider healthSlider; 

    // Thêm các Component cần thiết
    private Animator anim;
    private Collider2D playerCollider;
    private PlayerMovement playerMovement; 
    private Rigidbody2D rb; // <--- KHAI BÁO THÊM RIGIDBODY2D

    void Start()
    {
        // Khởi tạo các Component
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        playerMovement = GetComponent<PlayerMovement>(); 
        rb = GetComponent<Rigidbody2D>(); // <--- LẤY THAM CHIẾU
        
        // Kiểm tra an toàn
        if (rb == null) Debug.LogError("Rigidbody2D component missing on Player!");

        currentHealth = maxHealth;
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; 

        currentHealth -= damageAmount;

        // Cập nhật UI
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // Kiểm tra điều kiện thua
        if (currentHealth <= 0)
        {
            Die(); 
        }
        else
        {
            // TODO: Bạn có thể thêm animation "Hurt" hoặc "Flinch" ở đây
        }
    }

    void Die()
    {
        // 1. CHUYỂN TRẠNG THÁI CHẾT (Animator)
        if (anim != null)
        {
            anim.SetBool("IsDead", true); 
        }

        // 2. NGĂN CHẶN MỌI LỰC TÁC ĐỘNG & DI CHUYỂN (ĐÃ SỬA LỖI CẢNH BÁO)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;      // Dừng mọi chuyển động
            
            // SỬ DỤNG bodyType.Kinematic THAY CHO isKinematic = true
            rb.bodyType = RigidbodyType2D.Kinematic;
            
            // Nếu muốn hoàn toàn loại bỏ vật lý:
            rb.gravityScale = 0;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false; // Tắt script di chuyển
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = false; // Tắt Collider
        }
        
        Invoke("RestartCurrentScene", 3f); 
    }

    void RestartCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

}