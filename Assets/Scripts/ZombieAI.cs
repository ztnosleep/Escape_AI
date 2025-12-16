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
    public LayerMask obstacleLayer; 

    [Header("Swarm Mechanics (Bầy đàn)")]
    public float shoutRadius = 50f; // Bán kính tiếng hét gọi bầy
    public LayerMask zombieLayer;   // Layer của Zombie (để nhận diện đồng loại)
    private bool hasShouted = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip alertSound;

    [Header("Attack")]
    public float attackDuration = 0.5f;
    public float attackCooldown = 1.5f;

    [Header("Patrol")]
    public float patrolWaitTime = 1f;

    // --- CÁC BIẾN PRIVATE ---
    private AIPath aiPath;        
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
        
        // 1. TRẠNG THÁI TẤN CÔNG (Tối ưu nhất)
        if (distanceToPlayer <= attackRange && canSee)
        {
            aiPath.isStopped = true; 
            aiPath.destination = transform.position; // Dừng lại

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackCooldown;
            }
            
            // Cập nhật vị trí Player và duy trì trạng thái truy đuổi
            lastKnownPlayerPos = player.position;
            chaseTimer = chaseMemoryTime; 
        }
        
        // 2. TRUY ĐUỔI (CHASE) - Dù thấy hay chỉ nhớ vị trí
        else if (canSee || chaseTimer > 0f)
        {
            aiPath.isStopped = false; 

            // --- LOGIC KÍCH HOẠT BẦY ĐÀN ---
            // Nếu vừa mới nhìn thấy Player và chưa hét
            if (canSee && !hasShouted)
            {
                AlertNearbyZombies(); // Gọi đồng bọn
                PlayAlertSound();     // Phát tiếng hú
                hasShouted = true;    // Đánh dấu đã hét rồi
            }

            if (canSee)
            {
                // Nếu thấy Player, cập nhật vị trí và reset bộ nhớ
                lastKnownPlayerPos = player.position;
                chaseTimer = chaseMemoryTime; 
            }
            else
            {
                // Nếu không thấy (chaseTimer > 0f), giảm bộ nhớ
                chaseTimer -= Time.deltaTime; 
            }

            // Đi đến vị trí cuối cùng được biết
            aiPath.destination = lastKnownPlayerPos;
        }
        
        // 3. TUẦN TRA (PATROL) - Khi không thấy và hết bộ nhớ
        else
        {
            aiPath.isStopped = false;
            hasShouted = false; // Reset lại tiếng hét 
            Patrol();
        }

        // --- XỬ LÝ ANIMATION & HÌNH ẢNH ---
        if (aiPath.desiredVelocity.x > 0.01f) FlipSprite(true);
        else if (aiPath.desiredVelocity.x < -0.01f) FlipSprite(false);

        anim.SetBool("IsWalking", aiPath.velocity.magnitude > 0.1f);
    }
    
    void AlertNearbyZombies()
    {
        Collider2D[] zombies = Physics2D.OverlapCircleAll(transform.position, shoutRadius, zombieLayer);
        
        foreach (Collider2D z in zombies)
        {
            // Bỏ qua chính mình
            if (z.gameObject == gameObject) continue; 

            ZombieAI comrade = z.GetComponent<ZombieAI>();
            
            if (comrade != null)
            {
                // Gửi vị trí Player để đồng đội biết đường đến
                comrade.ReceiveAlert(player.position); 
            }
        }
    }
    
    // ⭐ HÀM NHẬN CẢNH BÁO ĐÃ SỬA ĐỔI
    public void ReceiveAlert(Vector3 targetPos)
    {
        // Nếu zombie đang bận đánh nhau hoặc đang truy đuổi rồi thì thôi
        // Logic này đảm bảo zombie đang Patrol sẽ chuyển sang Chase
        if (chaseTimer > 0f) return;
        
        // Debug.Log(gameObject.name + " : Đã nghe thấy tiếng gọi! Đang chuyển sang truy đuổi.");
        
        // Kích hoạt trạng thái đuổi
        lastKnownPlayerPos = targetPos;
        chaseTimer = chaseMemoryTime; // Cài đặt thời gian truy đuổi
        
        // Bắt đầu di chuyển ngay lập tức
        aiPath.isStopped = false;     
        aiPath.destination = lastKnownPlayerPos; 
    }
    
    void PlayAlertSound()
    {
        if (audioSource != null && alertSound != null)
        {
            audioSource.PlayOneShot(alertSound);
        }
    }
    
    void Patrol()
    {
        aiPath.destination = targetWaypoint.position;

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
        if (playerHealthScript != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            playerHealthScript.TakeDamage(1);
        }
    }

    void StopAttackingAnimation()
    {
        anim.SetBool("IsAttacking", false);
        aiPath.isStopped = false; 
    }

    bool CanSeePlayer(float distance)
    {
        if (distance > chaseRange) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        
        // Bắn Raycast kiểm tra tường
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);

        return hit.collider == null; 
    }

    void FlipSprite(bool facingRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = new Color(1, 0.5f, 0, 1); 
        Gizmos.DrawWireSphere(transform.position, shoutRadius); 

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}