using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Cần thiết để sử dụng Coroutines

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private float baseMoveSpeed; // Tốc độ gốc để trở về sau khi buff/debuff hết
    
    [Header("Buff/Debuff Settings")]
    public float buffSpeedMultiplier = 1.5f; // Tăng 50% tốc độ
    public float debuffSpeedMultiplier = 0.5f; // Giảm 50% tốc độ
    public float effectDuration = 2f; // Thời gian hiệu lực (2 giây)

    [Header("Audio Settings")]
    public AudioSource audioSource; // Kéo component AudioSource vào đây
    public AudioClip walkSound;     // Kéo file âm thanh bước chân vào đây
    public AudioClip shootSound;
    // [THÊM MỚI] Âm thanh Buff/Debuff
    public AudioClip buffSound;
    public AudioClip debuffSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Coroutine speedChangeCoroutine; // Biến để theo dõi Coroutine đang chạy

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // LƯU TỐC ĐỘ GỐC KHI BẮT ĐẦU GAME
        baseMoveSpeed = moveSpeed; 

        if (GameManager.Instance != null)
    {
        // Giả sử mỗi cấp tăng 0.5 tốc độ
        float bonusSpeed = (GameManager.Instance.speedLevel - 1) * 0.5f;
        UpgradeBaseSpeed(bonusSpeed);
    }
        // Tự động lấy AudioSource nếu chưa kéo vào (để tránh lỗi quên kéo)
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the player object! Cannot flip sprite.");
        }
    }

    void FixedUpdate()
    {
        // Di chuyển vật lý
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);

        // Xử lý Xoay và Lật nhân vật
        HandleRotationAndFlip();

        // Xử lý Âm thanh di chuyển
        HandleMovementAudio();
    }

    // Xử lý Lật và Xoay (GIỮ NGUYÊN)
    void HandleRotationAndFlip()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (spriteRenderer != null)
            {
                if (moveInput.x < 0)
                {
                    spriteRenderer.flipY = true;
                }
                else
                {
                    spriteRenderer.flipY = false;
                }
            }
        }
    }

    // XỬ LÝ VA CHẠM TRIGGER (THÊM MỚI)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Buff"))
        {
            // Bắt đầu hoặc khởi động lại Coroutine Buff
            TriggerSpeedChange(buffSpeedMultiplier, buffSound);
            Destroy(other.gameObject); // Xóa vật phẩm Buff
        }
        else if (other.CompareTag("Debuff"))
        {
            // Bắt đầu hoặc khởi động lại Coroutine Debuff
            TriggerSpeedChange(debuffSpeedMultiplier, debuffSound);
            Destroy(other.gameObject); // Xóa vật phẩm Debuff
        }
    }

    // HÀM CHUNG KHỞI ĐỘNG HIỆU ỨNG TỐC ĐỘ (THÊM MỚI)
    void TriggerSpeedChange(float multiplier, AudioClip sound)
    {
        // Nếu Coroutine đang chạy, dừng nó lại
        if (speedChangeCoroutine != null)
        {
            StopCoroutine(speedChangeCoroutine);
        }
        
        // Khởi động Coroutine mới
        speedChangeCoroutine = StartCoroutine(ChangeSpeedTemporarily(multiplier));
        PlayOneShotSound(sound);
    }

    // COROUTINE XỬ LÝ THỜI GIAN HIỆU LỰC (THÊM MỚI)
    IEnumerator ChangeSpeedTemporarily(float multiplier)
    {
        // 1. Áp dụng tốc độ mới
        moveSpeed = baseMoveSpeed * multiplier;
        Debug.Log("Speed changed to: " + moveSpeed);

        // 2. Chờ đợi hết thời gian hiệu lực (2 giây)
        yield return new WaitForSeconds(effectDuration);

        // 3. Đưa tốc độ về lại tốc độ gốc
        moveSpeed = baseMoveSpeed;
        Debug.Log("Speed reset to: " + moveSpeed);
        
        // Đặt Coroutine về null để biết nó đã kết thúc
        speedChangeCoroutine = null; 
    }

    // Xử lý Âm thanh di chuyển (GIỮ NGUYÊN)
    void HandleMovementAudio()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            if (!audioSource.isPlaying || audioSource.clip != walkSound)
            {
                audioSource.clip = walkSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying && audioSource.clip == walkSound)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }
    }

    // Hàm được Player Input gọi đến để cập nhật Input di chuyển (GIỮ NGUYÊN)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    // Hàm xử lý bắn (GIỮ NGUYÊN)
    public void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            PlayOneShotSound(shootSound, 0.7f);
            // ShootBullet(); 
        }
    }
    public void UpgradeBaseSpeed(float extraSpeed)
{
    baseMoveSpeed += extraSpeed;
    moveSpeed = baseMoveSpeed; // Cập nhật ngay lập tức
}

    // Hàm PlayOneShotSound đã được tinh gọn
    void PlayOneShotSound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume); 
        }
    }
}