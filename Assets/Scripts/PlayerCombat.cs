using UnityEngine;
using UnityEngine.InputSystem;
// (Không cần System.Collections.IEnumerator nếu bạn dùng ForceMode2D.Impulse)

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRate = 3f; 
    
    // Đã loại bỏ bulletPrefab và targetingRange khỏi logic sử dụng (Giữ để không báo lỗi Inspector)
    public GameObject bulletPrefab; 
    public float targetingRange = 10f; 

    [Header("Melee Knockback & Damage")]
    public int meleeDamage = 1; // Sát thương gây ra khi tấn công gần
    public float meleeKnockbackForce = 5f; // Lực đẩy
    public float meleeRange = 1.5f; // Tầm gần tối đa để áp dụng Knockback (Nên đặt nhỏ)

    private float nextAttackTime = 0f;
    private Animator anim;
    private PlayerMovement playerMovement; 

    void Start()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        
        // Bỏ kiểm tra Prefab nếu không dùng đạn.
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

        // 1. TÌM ZOMBIE GẦN NHẤT
        Transform target = FindNearestZombie();
        
        // 2. XỬ LÝ KNOCKBACK VÀ SÁT THƯƠNG
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            
            // Nếu kẻ thù ở quá gần (trong phạm vi cận chiến), áp dụng Knockback 
            if (distanceToTarget <= meleeRange)
            {
                ApplyDamageAndKnockback(target.gameObject);
                
                // Xoay Player về phía mục tiêu khi tấn công
                Vector2 direction = (target.position - transform.position).normalized;
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
        }
        
        // 3. BỎ BẮN: Loại bỏ logic ShootBullet
    }

    void StopAttack()
    {
        anim.SetBool("IsAttacking", false);
    }
    
    // --- HÀM TÌM KIẾM MỤC TIÊU ---
    // Giữ nguyên (Dùng targetingRange để quét)
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

    // --- HÀM TRỪ MÁU VÀ KNOCKBACK (ĐÃ CẬP NHẬT) ---
    // Thay thế cho ApplyMeleeKnockback và tích hợp logic trừ máu qua ZombieHealth
    void ApplyDamageAndKnockback(GameObject enemy)
    {
        // Lấy script quản lý máu của Zombie
        ZombieHealth zombieHealth = enemy.GetComponent<ZombieHealth>();

        if (zombieHealth != null)
        {
            // Hướng đẩy là hướng từ Player ra Enemy
            Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized; 
            
            // GỌI HÀM TRỪ MÁU & ĐẨY LÙI TỪ ZOMBIEHEALTH
            // Logic trừ máu, đếm 3 lần đẩy và gọi chết sẽ nằm trong ZombieHealth
            zombieHealth.TakeDamageAndKnockback(meleeDamage, knockbackDirection, meleeKnockbackForce);
        }
        else
        {
             Debug.LogWarning("Enemy " + enemy.name + " is missing ZombieHealth script!");
        }
    }
    
    // --- HÀM BẮN ĐÃ ĐƯỢC LOẠI BỎ/DỌN DẸP ---
    // Loại bỏ hàm ShootBullet và CreateSimpleCircleSprite
}