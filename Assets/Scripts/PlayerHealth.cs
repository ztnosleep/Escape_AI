using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class PlayerHealth : MonoBehaviour
{
    [Header("Cài đặt Máu")]
    public int maxHealth = 3;
    private int currentHealth;

    public Slider healthSlider; 

    [Header("Cài đặt Âm thanh")] // [THÊM MỚI]
    public AudioClip hurtSound;  // Kéo file tiếng bị thương
    public AudioClip deathSound; // Kéo file tiếng chết
    private AudioSource audioSource; // Biến loa

    // Thêm các Component cần thiết
    private Animator anim;
    private Collider2D playerCollider;
    private PlayerMovement playerMovement; 
    private Rigidbody2D rb; 

    void Start()
    {
        // Khởi tạo các Component
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        playerMovement = GetComponent<PlayerMovement>(); 
        rb = GetComponent<Rigidbody2D>(); 
        
        // [THÊM MỚI] Lấy AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Nếu quên gắn AudioSource thì code tự thêm vào để tránh lỗi
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
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
            // [THÊM MỚI] Nếu chưa chết -> Phát tiếng bị thương
            PlaySound(hurtSound);
            
            // TODO: Bạn có thể thêm animation "Hurt" hoặc "Flinch" ở đây
            // if (anim != null) anim.SetTrigger("Hurt");
        }
    }

    void Die()
    {
        // [THÊM MỚI] Phát tiếng chết
        PlaySound(deathSound);

        // 1. CHUYỂN TRẠNG THÁI CHẾT (Animator)
        if (anim != null)
        {
            anim.SetBool("IsDead", true); 
        }

        // 2. NGĂN CHẶN MỌI LỰC TÁC ĐỘNG & DI CHUYỂN
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;      // Dừng mọi chuyển động
            rb.bodyType = RigidbodyType2D.Kinematic; // Ngừng tác động vật lý va chạm
            rb.gravityScale = 0; // Không bị rơi
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

    // [THÊM MỚI] Hàm phụ trợ để phát âm thanh
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void RestartCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}