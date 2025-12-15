using UnityEngine;
using Pathfinding; // Bắt buộc cho A*

public class ZombieAI : MonoBehaviour
{
    [Header("References")]
    public Transform pointA;
    public Transform pointB;
    public Transform player;

    [Header("Settings")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float chaseRange = 20f; 
    public float chaseMemoryTime = 3f; // Thời gian đuổi theo sau khi mất dấu

    [Header("Detection")]
    public LayerMask obstacleLayer; // ⭐ Quan trọng: Cần gán Layer "Wall" vào đây

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackSound;

    [Header("Attack")]
    public float attackDuration = 0.5f;
    public float attackCooldown = 1.5f;

    [Header("Patrol")]
    public float patrolWaitTime = 1f;

    // --- CÁC BIẾN PRIVATE ---
    private AIPath aiPath;        // Thay thế cho NavMeshAgent
    private Animator anim;
    private PlayerHealth playerHealthScript;
    
    private Transform targetWaypoint; 
    private Vector3 lastKnownPlayerPos;
    
    private float chaseTimer = 0f;
    private float nextAttackTime = 0f;
    private float currentWaitTime;

    void Start()
    {
        anim = GetComponent<Animator>();
        targetWaypoint = pointA;
        currentWaitTime = patrolWaitTime;

        // 1. LẤY COMPONENT AIPath (Của A* Project)
        aiPath = GetComponent<AIPath>();
        if (aiPath == null) Debug.LogError("Thiếu component 'AIPath' trên Zombie!");
        else aiPath.maxSpeed = moveSpeed; // Đồng bộ tốc độ

        // 2. Setup Audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; 
        }

        // 3. Setup Player
        if (player != null)
        {
            playerHealthScript = player.GetComponent<PlayerHealth>();
        }
    }
    
    void Update()
    {
        if (player == null || aiPath == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer(distanceToPlayer);

        // --- LOGIC FSM (State Machine) ---

        // 1. TRẠNG THÁI TẤN CÔNG
        if (distanceToPlayer <= attackRange && canSee)
        {
            aiPath.isStopped = true; // Dừng lại để đánh
            aiPath.destination = transform.position; // Đảm bảo không trượt đi

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackCooldown;
            }
            
            // Cập nhật vị trí lần cuối thấy player
            lastKnownPlayerPos = player.position;
            chaseTimer = chaseMemoryTime;
        }
        // 2. TRẠNG THÁI TRUY ĐUỔI (Thấy hoặc còn nhớ vị trí)
        else if (canSee || chaseTimer > 0f)
        {
            aiPath.isStopped = false; // Cho phép đi

            if (canSee)
            {
                lastKnownPlayerPos = player.position;
                chaseTimer = chaseMemoryTime; // Reset bộ nhớ nếu đang nhìn thấy
            }
            else
            {
                chaseTimer -= Time.deltaTime; // Giảm thời gian nhớ nếu mất dấu
            }

            // Di chuyển đến vị trí (thực hoặc vị trí nhớ)
            aiPath.destination = lastKnownPlayerPos;
        }
        // 3. TRẠNG THÁI TUẦN TRA
        else
        {
            aiPath.isStopped = false;
            Patrol();
        }

        // --- XỬ LÝ ANIMATION & HÌNH ẢNH ---
        
        // Flip Sprite dựa trên hướng A* muốn đi (desiredVelocity)
        if (aiPath.desiredVelocity.x > 0.01f) FlipSprite(true);
        else if (aiPath.desiredVelocity.x < -0.01f) FlipSprite(false);

        // Set Animation Walk
        // aiPath.velocity.magnitude là vận tốc thực tế
        anim.SetBool("IsWalking", aiPath.velocity.magnitude > 0.1f);
    }

    void Patrol()
    {
        // Gán đích đến là Waypoint
        aiPath.destination = targetWaypoint.position;

        // Kiểm tra xem đã đến nơi chưa (A* hỗ trợ sẵn biến reachedDestination)
        if (aiPath.reachedDestination)
        {
            if (currentWaitTime <= 0f)
            {
                // Hết thời gian chờ -> Đổi điểm
                targetWaypoint = (targetWaypoint == pointA) ? pointB : pointA;
                currentWaitTime = patrolWaitTime;
            }
            else
            {
                // Đang chờ
                currentWaitTime -= Time.deltaTime;
            }
        }
    }

    void AttackPlayer()
    {
        anim.SetBool("IsAttacking", true);

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Invoke("ApplyDamageToPlayer", attackDuration);
        Invoke("StopAttackingAnimation", attackDuration + 0.1f);
    }

    void ApplyDamageToPlayer()
    {
        // Kiểm tra lại khoảng cách cho chắc chắn trước khi trừ máu
        if (playerHealthScript != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            playerHealthScript.TakeDamage(1);
        }
    }

    void StopAttackingAnimation()
    {
        anim.SetBool("IsAttacking", false);
        aiPath.isStopped = false; // Đánh xong thì cho phép di chuyển lại
    }

    // Kiểm tra tầm nhìn có bị tường chắn không
    bool CanSeePlayer(float distance)
    {
        if (distance > chaseRange) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        
        // Bắn Raycast kiểm tra tường
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);

        // Nếu hit.collider != null nghĩa là trúng tường -> return false (không thấy)
        // Nếu hit.collider == null nghĩa là đường thoáng -> return true (thấy)
        return hit.collider == null; 
    }

    void FlipSprite(bool facingRight)
    {
        Vector3 scale = transform.localScale;
        // Giữ nguyên độ lớn, chỉ đổi dấu
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}