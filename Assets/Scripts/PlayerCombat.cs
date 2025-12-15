using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRate = 3f; 
    
    // Đã loại bỏ bulletPrefab và targetingRange khỏi logic sử dụng
    public GameObject bulletPrefab; 
    public float targetingRange = 10f; 

    [Header("Audio Settings")]
    public AudioClip attackSound; // Kéo file âm thanh tấn công vào đây
    // ⭐ [THÊM MỚI] Âm thanh khi chạm vật phẩm Defense
    public AudioClip defenseSound; 
    
    private AudioSource audioSource; // Biến để lấy cái loa

    [Header("Melee Knockback & Damage")]
    public int meleeDamage = 1; 
    public float meleeKnockbackForce = 5f; 
    public float meleeRange = 1.5f; 

    private float nextAttackTime = 0f;
    private Animator anim;
    private PlayerMovement playerMovement; 

    void Start()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        
        // Lấy hoặc thêm AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (GameManager.Instance != null)
        {
            // Mỗi cấp cộng thêm 1 Damage
            int bonusDamage = GameManager.Instance.damageLevel - 1;
            meleeDamage += bonusDamage;
        }
    }
    
    public void UpgradeDamage(int extraDamage)
    {
        meleeDamage += extraDamage;
        Debug.Log("Damage upgraded! Current Damage: " + meleeDamage);
    }

    // ⭐ HÀM PHÁT HIỆN VA CHẠM VỚI VẬT PHẨM (Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("defense")) 
        {
            // Gọi hàm tăng sát thương (nếu cần)
            IncreaseMeleeDamage(1); 
            
            // ⭐ PHÁT ÂM THANH DEFENSE
            PlayOneShotSound(defenseSound);
            
            Destroy(other.gameObject);
        }
    }

    // Hàm PlayOneShotSound được tái sử dụng từ các script khác (nên có)
    void PlayOneShotSound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume); 
        }
    }

    // Gọi từ Input System khi bấm phím K
    public void OnAttack(InputValue value)
    {
        if (value.isPressed && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + (1f / attackRate);
        }
    }

    void Attack()
    {
        // Kiểm tra an toàn
        if (anim == null) { Debug.LogError("Animator component missing!"); return; } 

        anim.SetBool("IsAttacking", true);
        Invoke(nameof(StopAttack), 0.1f);

        // PHÁT TIẾNG TẤN CÔNG
        PlayOneShotSound(attackSound); // Đổi từ PlayAttackSound sang PlayOneShotSound

        // 1. TÌM ZOMBIE GẦN NHẤT
        Transform target = FindNearestZombie();
        
        // 2. XỬ LÝ KNOCKBACK VÀ SÁT THƯƠNG
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            
            if (distanceToTarget <= meleeRange)
            {
                ApplyDamageAndKnockback(target.gameObject);
                
                // ⭐ Cẩn thận với đoạn code xoay:
                // Nếu đây là game 2D top-down, xoay (rotation) này là đúng.
                // Nếu là game 2D side-scrolling, nó có thể gây lỗi phóng to/biến dạng
                // (như lỗi bạn đã gặp trước đây)
                Vector2 direction = (target.position - transform.position).normalized;
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
        }
    }

    // HÀM CŨ ĐƯỢC THAY BẰNG PlayOneShotSound
    /* void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
    */
    
    public void IncreaseMeleeDamage(int amount)
    {
        meleeDamage += amount;
        Debug.Log("Player Melee Damage increased! New Damage: " + meleeDamage);
    }

    void StopAttack()
    {
        if (anim != null)
        {
            anim.SetBool("IsAttacking", false);
        }
    }
    
    // --- CÁC HÀM TÌM KIẾM/ÁP DỤNG KNOCKBACK GIỮ NGUYÊN ---
    Transform FindNearestZombie()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, targetingRange, LayerMask.GetMask("Enemy"));
        Transform nearestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Enemy") || hit.gameObject.layer == LayerMask.NameToLayer("Enemy")) 
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTarget = hit.transform;
                }
            }
        }
        return nearestTarget;
    }

    void ApplyDamageAndKnockback(GameObject enemy)
    {
        ZombieHealth zombieHealth = enemy.GetComponent<ZombieHealth>();

        if (zombieHealth != null)
        {
            Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized; 
            zombieHealth.TakeDamageAndKnockback(meleeDamage, knockbackDirection, meleeKnockbackForce);
        }
        else
        {
             Debug.LogWarning("Enemy " + enemy.name + " is missing ZombieHealth script!");
        }
    }
}