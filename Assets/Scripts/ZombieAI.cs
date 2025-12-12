using UnityEngine;
using UnityEngine.SceneManagement;

public class ZombieAI : MonoBehaviour
{
    // --- CÁC BIẾN CÔNG KHAI CẦN THIẾT LẬP TRONG UNITY INSPECTOR ---
    
    // Waypoints và Người chơi
    public Transform pointA; 
    public Transform pointB; 
    public Transform player; 

    // Cài đặt Tốc độ và Phạm vi
    public float moveSpeed = 2f; 
    public float attackRange = 1.5f; // Phạm vi chuyển sang trạng thái Tấn công
    public float chaseRange = 3f;    // Phạm vi phát hiện và bắt đầu Truy đuổi
    
    // Logic Tránh vật cản (Raycasting)
    public float obstacleCheckDistance = 0.5f; // Khoảng cách quét tìm vật cản
    public LayerMask whatIsGround; // Layer chứa Tường/Nền đất (Cần thiết lập trong Inspector!)
    
    // Logic Tấn công và Tuần tra
    public float attackDuration = 0.75f; // ĐỘ TRỄ MỚI: Gây sát thương sau 0.75 giây
    public float attackCooldown = 1.0f; 
    public float patrolWaitTime = 1f; 
    
    // Logic Truy đuổi
    public float chaseDuration = 2f; 
    private float chaseTimer = 0f;  
    
    // --- BIẾN PRIVATE VÀ THAM CHIẾU ---
    
    private Animator anim;
    private Transform targetWaypoint; 
    private PlayerHealth playerHealthScript; 
    private float nextAttackTime = 0f; 
    private float currentWaitTime;

    
    // =========================================================================
    // KHỞI TẠO
    // =========================================================================
    
    void Start()
    {
        anim = GetComponent<Animator>();
        targetWaypoint = pointA;
        currentWaitTime = patrolWaitTime;
        
        // TÌM SCRIPT PlayerHealth
        if (player != null)
        {
            playerHealthScript = player.GetComponent<PlayerHealth>();
        } 
        
        if (playerHealthScript == null)
        {
            Debug.LogError("PlayerHealth script not found on the player object! Hãy đảm bảo PlayerHealth được đính kèm vào GameObject Player.");
        }
    }

    // =========================================================================
    // LOGIC CẬP NHẬT TRẠNG THÁI
    // =========================================================================

    void Update()
    {
        // Kiểm tra bảo vệ lỗi
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // A. TRẠNG THÁI 1: TẤN CÔNG (Attack)
            chaseTimer = chaseDuration; 
            
            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else if (distanceToPlayer <= chaseRange || chaseTimer > 0)
        {
            // B. TRẠNG THÁI 2: TRUY ĐUỔI (Chase)
            
            if (distanceToPlayer <= chaseRange)
            {
                chaseTimer = chaseDuration; // Reset timer nếu người chơi vẫn còn trong phạm vi
            }
            
            if (chaseTimer > 0)
            {
                // Sử dụng hàm di chuyển có tránh va chạm
                MoveWithAvoidance(player.position, true);
                chaseTimer -= Time.deltaTime;
            }
            else
            {
                Patrol(); 
            }
        }
        else
        {
            // C. TRẠNG THÁI 3: TUẦN TRA (Patrol)
            Patrol();
        }
    }

    // =========================================================================
    // LOGIC DI CHUYỂN & TRÁNH VẬT CẢN (Raycasting)
    // =========================================================================

    void MoveWithAvoidance(Vector2 targetPosition, bool isChasing)
    {
        anim.SetBool("IsAttacking", false); 
        
        // 1. Lật Sprite (Quay mặt về phía đích)
        FlipSprite(targetPosition.x > transform.position.x);

        // 2. KIỂM TRA VẬT CẢN TRƯỚC MẶT (Raycasting)
        // Lấy hướng mà zombie đang nhìn
        Vector2 checkDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left; 

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            checkDirection, 
            obstacleCheckDistance, 
            whatIsGround
        );
        
        // Vẽ Raycast để dễ dàng Debug trong Scene View
        Debug.DrawRay(transform.position, checkDirection * obstacleCheckDistance, Color.yellow);
        
        bool obstacleAhead = hit.collider != null;

        if (!obstacleAhead)
        {
             // 3. DI CHUYỂN: Nếu KHÔNG có vật cản, tiếp tục di chuyển về phía đích
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        else if (!isChasing)
        {
            // Nếu đang Patrol và bị chặn, buộc đổi Waypoint ngay
            HandleWaypointChange();
        }
        
        // 4. Xử lý logic đổi Waypoint cho trạng thái Patrol (khi đến đích)
        if (!isChasing && Vector2.Distance(transform.position, targetWaypoint.position) <= 0.1f)
        {
             HandleWaypointChange();
        }
    }

    // --- Phương thức Tuần tra (Sử dụng MoveWithAvoidance) ---
    void Patrol()
    {
        float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint.position);

        if (distanceToWaypoint > 0.1f) 
        {
            // Di chuyển bằng hàm có kiểm tra vật cản
            MoveWithAvoidance(targetWaypoint.position, false);
        }
        else 
        {
            // Xử lý chờ và đổi Waypoint
            HandleWaypointChange();
        }
    }
    
    // --- Xử lý logic đổi Waypoint ---
    void HandleWaypointChange()
    {
        if (currentWaitTime <= 0)
        {
            // Đổi Waypoint
            targetWaypoint = (targetWaypoint == pointA) ? pointB : pointA;
            currentWaitTime = patrolWaitTime; 
        }
        else
        {
            currentWaitTime -= Time.deltaTime; 
        }
    }
    
    // --- Phương thức Hỗ trợ Lật Sprite ---
    void FlipSprite(bool movingRight)
    {
        if (movingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // =========================================================================
    // LOGIC TẤN CÔNG ĐÃ ĐƯỢC SỬA ĐỔI (Kiểm tra lại vị trí sau 0.75s)
    // =========================================================================

    void AttackPlayer()
    {
        anim.SetBool("IsAttacking", true); 
        
        Invoke("ApplyDamageToPlayer", attackDuration);
        
        Invoke("StopAttackingAnimation", attackDuration + 0.1f);
    }

    // Hàm gọi trừ máu người chơi (Chỉ gây sát thương nếu người chơi vẫn trong tầm)
    void ApplyDamageToPlayer()
    {
        // CHỈ GÂY SÁT THƯƠNG NẾU NGƯỜI CHƠI VẪN TRONG PHẠM VI TẤN CÔNG
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            if (playerHealthScript != null)
            {
                playerHealthScript.TakeDamage(1); 
            }
        }
    }
    
    void StopAttackingAnimation()
    {
        anim.SetBool("IsAttacking", false); 
    }
}