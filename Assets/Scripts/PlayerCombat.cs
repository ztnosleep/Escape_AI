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
    public AudioClip attackSound; // [THÊM MỚI] Kéo file âm thanh tấn công vào đây
    private AudioSource audioSource; // [THÊM MỚI] Biến để lấy cái loa

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
        
        // [THÊM MỚI] Lấy AudioSource đang nằm trên cùng GameObject này
        audioSource = GetComponent<AudioSource>();

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
        anim.SetBool("IsAttacking", true);
        Invoke(nameof(StopAttack), 0.1f);

        // [THÊM MỚI] PHÁT TIẾNG TẤN CÔNG
        PlayAttackSound();

        // 1. TÌM ZOMBIE GẦN NHẤT
        Transform target = FindNearestZombie();
        
        // 2. XỬ LÝ KNOCKBACK VÀ SÁT THƯƠNG
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            
            if (distanceToTarget <= meleeRange)
            {
                ApplyDamageAndKnockback(target.gameObject);
                
                Vector2 direction = (target.position - transform.position).normalized;
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
        }
    }

    // [THÊM MỚI] Hàm xử lý phát âm thanh
    void PlayAttackSound()
    {
        // Kiểm tra xem có AudioSource và File âm thanh chưa để tránh lỗi
        if (audioSource != null && attackSound != null)
        {
            // PlayOneShot giúp âm thanh chồng lên nhau (không bị ngắt quãng nhạc nền)
            audioSource.PlayOneShot(attackSound);
        }
    }

    void StopAttack()
    {
        anim.SetBool("IsAttacking", false);
    }
    
    // --- CÁC HÀM CŨ GIỮ NGUYÊN ---
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