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

    [Header("Swarm Mechanics (Bầy đàn)")]
    public float shoutRadius = 50f; // Bán kính tiếng hét gọi bầy
    public LayerMask zombieLayer;   // Layer của Zombie (để nhận diện đồng loại)
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
        // 1. TẤN CÔNG
        if (distanceToPlayer <= attackRange && canSee)
        {
            aiPath.isStopped = true; 
            aiPath.destination = transform.position;

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackCooldown;
            }
            
            lastKnownPlayerPos = player.position;
            chaseTimer = chaseMemoryTime;
        }
        // 2. TRUY ĐUỔI (CHASE)
        else if (canSee || chaseTimer > 0f)
        {
            aiPath.isStopped = false; 

            // --- [MỚI] LOGIC KÍCH HOẠT BẦY ĐÀN ---
            // Nếu vừa mới nhìn thấy Player và chưa hét
            if (canSee && !hasShouted)
            {
                AlertNearbyZombies(); // Gọi đồng bọn
                PlayAlertSound();     // Phát tiếng hú
                hasShouted = true;    // Đánh dấu đã hét rồi
            }
            // --------------------------------------

            if (canSee)
            {
                lastKnownPlayerPos = player.position;
                chaseTimer = chaseMemoryTime; 
            }
            else
            {
                chaseTimer -= Time.deltaTime; 
            }

            aiPath.destination = lastKnownPlayerPos;
        }
        // 3. TUẦN TRA (PATROL)
        else
        {
            aiPath.isStopped = false;
            hasShouted = false; // Reset lại tiếng hét để lần sau thấy lại hét tiếp
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
    void AlertNearbyZombies()
    {
        Debug.Log("--- BẮT ĐẦU QUÉT TÌM ĐỒNG BỌN ---");

        // 1. Thử quét TẤT CẢ các layer xem có chạm vào cái gì không (để test bán kính)
        Collider2D[] allHits = Physics2D.OverlapCircleAll(transform.position, shoutRadius);
        Debug.Log("Tổng số vật thể trong bán kính (Bất kể layer nào): " + allHits.Length);

        // 2. Quét chính thức theo Layer Zombie
        Collider2D[] zombies = Physics2D.OverlapCircleAll(transform.position, shoutRadius, zombieLayer);
        Debug.Log("Số lượng ZOMBIE tìm thấy (Theo layer " + zombieLayer.value + "): " + zombies.Length);

        foreach (Collider2D z in zombies)
        {
            // Kiểm tra xem có phải chính mình không
            if (z.gameObject == gameObject) 
            {
                Debug.Log("- Bỏ qua: Chính là tôi.");
                continue;
            }

            Debug.Log("- Tìm thấy object: " + z.gameObject.name);

            // Thử lấy script
            ZombieAI comrade = z.GetComponent<ZombieAI>();
            
            if (comrade != null)
            {
                Debug.Log("  -> Đã tìm thấy Script ZombieAI! Gửi lệnh!");
                comrade.ReceiveAlert(player.position);
            }
            else
            {
                Debug.LogError("  -> LỖI: Tìm thấy Object nhưng không thấy Script ZombieAI! Kiểm tra xem Script và Collider có nằm cùng 1 GameObject không?");
            }
        }
    }
    public void ReceiveAlert(Vector3 targetPos)
    {
        // Nếu đang bận đánh nhau hoặc đang đuổi rồi thì thôi
        if (chaseTimer > 0) return;
        Debug.Log(gameObject.name + " : Đã nghe thấy tiếng gọi! Đang chuyển sang truy đuổi.");
        // Kích hoạt trạng thái đuổi
        lastKnownPlayerPos = targetPos;
        chaseTimer = chaseMemoryTime; // Gán thời gian đuổi
        
        // (Tùy chọn) Đồng bọn nhận lệnh cũng có thể hú lên để tạo hiệu ứng dây chuyền
        //PlayAlertSound(); 
        AlertNearbyZombies();
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

        Gizmos.color = new Color(1, 0.5f, 0, 1); // Màu cam (R=1, G=0.5, B=0)
        Gizmos.DrawWireSphere(transform.position, shoutRadius);

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}