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
    public float attackRange = 1.5f; 
    public float chaseRange = 3f;    
    
    // [THÊM MỚI] Cài đặt Âm thanh
    [Header("Audio Settings")]
    public AudioSource audioSource; // Kéo AudioSource của Zombie vào đây
    public AudioClip attackSound;   // Kéo tiếng gầm/tấn công của Zombie vào đây

    // Logic Tránh vật cản (Raycasting)
    public float obstacleCheckDistance = 0.5f; 
    public LayerMask whatIsGround; 
    
    // Logic Tấn công và Tuần tra
    public float attackDuration = 0.75f; 
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
        
        // [THÊM MỚI] Tự lấy AudioSource nếu quên kéo
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Nếu chưa có thì tự thêm vào để tránh lỗi
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f; // Chỉnh thành âm thanh 3D (nghe gần thì to, xa thì nhỏ)
            }
        }

        // TÌM SCRIPT PlayerHealth
        if (player != null)
        {
            playerHealthScript = player.GetComponent<PlayerHealth>();
        } 
        
        if (playerHealthScript == null)
        {
            Debug.LogError("PlayerHealth script not found on the player object!");
        }
    }

    // =========================================================================
    // LOGIC CẬP NHẬT TRẠNG THÁI
    // =========================================================================

    void Update()
    {
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
                chaseTimer = chaseDuration; 
            }
            
            if (chaseTimer > 0)
            {
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
        FlipSprite(targetPosition.x > transform.position.x);

        Vector2 checkDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left; 

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            checkDirection, 
            obstacleCheckDistance, 
            whatIsGround
        );
        
        Debug.DrawRay(transform.position, checkDirection * obstacleCheckDistance, Color.yellow);
        
        bool obstacleAhead = hit.collider != null;

        if (!obstacleAhead)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        else if (!isChasing)
        {
            HandleWaypointChange();
        }
        
        if (!isChasing && Vector2.Distance(transform.position, targetWaypoint.position) <= 0.1f)
        {
             HandleWaypointChange();
        }
    }

    void Patrol()
    {
        float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint.position);

        if (distanceToWaypoint > 0.1f) 
        {
            MoveWithAvoidance(targetWaypoint.position, false);
        }
        else 
        {
            HandleWaypointChange();
        }
    }
    
    void HandleWaypointChange()
    {
        if (currentWaitTime <= 0)
        {
            targetWaypoint = (targetWaypoint == pointA) ? pointB : pointA;
            currentWaitTime = patrolWaitTime; 
        }
        else
        {
            currentWaitTime -= Time.deltaTime; 
        }
    }
    
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
    // LOGIC TẤN CÔNG ĐÃ THÊM ÂM THANH
    // =========================================================================

    void AttackPlayer()
    {
        anim.SetBool("IsAttacking", true); 
        
        // [THÊM MỚI] Phát tiếng Zombie Gầm/Đánh ngay khi bắt đầu Animation
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Invoke("ApplyDamageToPlayer", attackDuration);
        Invoke("StopAttackingAnimation", attackDuration + 0.1f);
    }

    void ApplyDamageToPlayer()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            if (playerHealthScript != null)
            {
                // Khi dòng này chạy, PlayerHealth sẽ tự phát tiếng "Bị thương" (Hurt Sound)
                playerHealthScript.TakeDamage(1); 
            }
        }
    }
    
    void StopAttackingAnimation()
    {
        anim.SetBool("IsAttacking", false); 
    }
}