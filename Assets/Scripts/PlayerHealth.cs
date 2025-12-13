using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class PlayerHealth : MonoBehaviour
{
    [Header("Cài đặt Máu")]
    public int maxHealth = 3;
    private int currentHealth;

    public Slider healthSlider; 

    [Header("Cài đặt Âm thanh")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    // [THÊM MỚI] Âm thanh hồi máu
    public AudioClip healSound; 
    private AudioSource audioSource;

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
        
        // [CẬP NHẬT] Lấy AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
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
        // ... (Giữ nguyên nội dung hàm TakeDamage) ...
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
            // Nếu chưa chết -> Phát tiếng bị thương
            PlaySound(hurtSound);
            
            // TODO: Bạn có thể thêm animation "Hurt" hoặc "Flinch" ở đây
            // if (anim != null) anim.SetTrigger("Hurt");
        }
    }

    // ⭐ HÀM HỒI MÁU MỚI ⭐
    public void Heal(int healAmount, bool fullHeal)
    {
        if (currentHealth >= maxHealth) return; // Đã đầy máu thì không hồi nữa

        if (fullHeal)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += healAmount;
            // Đảm bảo máu không vượt quá mức tối đa
            currentHealth = Mathf.Min(currentHealth, maxHealth); 
        }

        // Cập nhật UI
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        
        // Phát tiếng hồi máu
        PlaySound(healSound);
    }
    
    // ⭐ PHÁT HIỆN VA CHẠM VỚI VẬT PHẨM (Trái tim) ⭐
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem vật thể va chạm có tag là "Heart_1" không (Hồi 1 máu)
        if (other.CompareTag("Heart_1")) 
        {
            Heal(1, false); // Hồi 1 máu
            Destroy(other.gameObject); // Xóa vật phẩm sau khi sử dụng
        }
        
        // Kiểm tra xem vật thể va chạm có tag là "Heart_Full" không (Hồi đầy máu)
        else if (other.CompareTag("Heart_Full")) 
        {
            Heal(0, true); // Hồi đầy máu (tham số 0 không quan trọng khi fullHeal là true)
            Destroy(other.gameObject); // Xóa vật phẩm sau khi sử dụng
        }
        
        // **LƯU Ý:** Bạn cần đảm bảo các vật phẩm trái tim được cài đặt đúng
        // - Có Collider2D được đặt là **Is Trigger**.
        // - Có **Tag** tương ứng ("Heart_1" hoặc "Heart_Full").
    }


    void Die()
    {
        // ... (Giữ nguyên nội dung hàm Die) ...
        PlaySound(deathSound);

        // 1. CHUYỂN TRẠNG THÁI CHẾT (Animator)
        if (anim != null)
        {
            anim.SetBool("IsDead", true); 
        }

        // 2. NGĂN CHẶN MỌI LỰC TÁC ĐỘNG & DI CHUYỂN
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        
        Invoke("RestartCurrentScene", 3f); 
    }

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