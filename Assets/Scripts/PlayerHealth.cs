using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using TMPro; // THÊM THƯ VIỆN CẦN THIẾT ĐỂ SỬ DỤNG TEXTMESHPRO

public class PlayerHealth : MonoBehaviour
{
    [Header("Cài đặt Máu")]
    public int maxHealth = 3;
    private int currentHealth;
    
    // UI Slider cho thanh máu
    public Slider healthSlider; 

    // [THÊM MỚI] UI Text cho giá trị HP (vd: 3/3)
    [Header("Cài đặt UI Máu")]
    public TextMeshProUGUI hpText; 

    [Header("Cài đặt Âm thanh")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
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
        
        // Lấy hoặc thêm AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Kiểm tra an toàn
        if (rb == null) Debug.LogError("Rigidbody2D component missing on Player!");

        currentHealth = maxHealth;
        
        // Khởi tạo UI Slider và Text
        InitializeUI();
    }

    // Hàm riêng biệt để khởi tạo UI
    void InitializeUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        // Cập nhật giá trị HP ban đầu (ví dụ: 3/3)
        UpdateHPText();
    }

    // HÀM CẬP NHẬT GIÁ TRỊ TEXT
    private void UpdateHPText()
    {
        if (hpText != null)
        {
            // Định dạng: Current Health / Max Health (vd: 2/5)
            hpText.text = currentHealth.ToString() + "/" + maxHealth.ToString();
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
        UpdateHPText(); // Cập nhật Text khi mất máu

        // Kiểm tra điều kiện thua
        if (currentHealth <= 0)
        {
            Die(); 
        }
        else
        {
            PlaySound(hurtSound);
        }
    }

    // HÀM HỒI MÁU
    public void Heal(int healAmount, bool fullHeal)
    {
        // Nếu đã đầy máu và không phải hồi đầy (fullHeal), thì thoát
        if (currentHealth >= maxHealth && !fullHeal) return; 

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
        UpdateHPText(); // Cập nhật Text khi hồi máu
        
        PlaySound(healSound);
    }
    
    // HÀM TĂNG MÁU TỐI ĐA VÀ HỒI MÁU
    public void IncreaseMaxHealth(int amount)
    {
        // 1. Tăng giới hạn máu tối đa
        maxHealth += amount;
        
        // 2. Tăng máu hiện tại (không vượt quá giới hạn mới)
        currentHealth += amount;
        
        // 3. Cập nhật thanh Slider UI (phải cập nhật MaxValue trước)
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        UpdateHPText(); // Cập nhật Text khi tăng Max Health

        // 4. Phát âm thanh
        PlaySound(healSound);

        Debug.Log("Max Health increased to: " + maxHealth + ". Current Health: " + currentHealth);
    }
    
    // PHÁT HIỆN VA CHẠM VỚI VẬT PHẨM (Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Heart_1")) 
        {
            Heal(1, false); // Hồi 1 máu
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Heart_Full")) 
        {
            Heal(0, true); // Hồi đầy máu
            Destroy(other.gameObject);
        }
    }

    void Die()
    {
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