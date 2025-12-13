using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [Header("Health & Stats")]
    public int maxHealth = 3;
    private int currentHealth;
    private int knockbackCount = 0; 

    [Header("Animation")]
    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D zombieCollider;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        zombieCollider = GetComponent<Collider2D>();
    }

    // HÀM ĐƯỢC GỌI TỪ PlayerCombat
    public void TakeDamageAndKnockback(int damageAmount, Vector2 knockbackDirection, float knockbackForce)
    {
        if (currentHealth <= 0) return; // Đã chết

        currentHealth -= damageAmount; // Trừ máu
        knockbackCount++; // Tăng số lần bị đẩy lùi
        
        // Áp dụng lực đẩy
        if (rb != null)
        {
            // Dùng Impulse để tạo cú hất (nhớ điều chỉnh knockbackForce trong Inspector)
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        // KIỂM TRA ĐIỀU KIỆN CHẾT
        if (currentHealth <= 0 || knockbackCount >= 3)
        {
            Die();
        }
    }

    void Die()
    {
        currentHealth = 0;
        
        if (anim != null)
        {
            // Kích hoạt animation chết
            anim.SetBool("IsDead", true); 
        }

        // Vô hiệu hóa vật lý và va chạm (ĐÃ SỬA LỖI CẢNH BÁO)
        if (rb != null) 
        {
            // Thay thế rb.isKinematic = true; bằng rb.bodyType = RigidbodyType2D.Kinematic;
            rb.bodyType = RigidbodyType2D.Kinematic; 
            rb.linearVelocity = Vector2.zero; // Đảm bảo vận tốc dừng ngay lập tức
        }
        
        if (zombieCollider != null) zombieCollider.enabled = false;
        
        gameObject.SetActive(false);

        // HỒI SINH SAU 5 GIÂY
        Invoke("Respawn", 5f);
    }
    void Respawn()
{
    // Reset máu và knockback
    currentHealth = maxHealth;
    knockbackCount = 0;

    // Reset collider và rigidbody
    if (zombieCollider != null) zombieCollider.enabled = true;
    if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

    // Reset animation
    if (anim != null)
    {
        anim.Rebind();       // Reset toàn bộ animator
        // anim.Update(0f);
        anim.SetBool("IsDead", false);
    }

    // Bật lại zombie
    gameObject.SetActive(true);
}

}