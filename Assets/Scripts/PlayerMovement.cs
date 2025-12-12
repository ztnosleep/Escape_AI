using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Audio Settings")]
    public AudioSource audioSource; // Kéo component AudioSource vào đây
    public AudioClip walkSound;     // Kéo file âm thanh bước chân vào đây
    public AudioClip shootSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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

        // Xử lý Âm thanh di chuyển (Thêm mới)
        HandleMovementAudio();
    }

    // Xử lý Lật và Xoay
    void HandleRotationAndFlip()
    {
        // Chỉ xử lý nếu có Input di chuyển
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // TÍNH GÓC XOAY
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

            // ÁP DỤNG XOAY Z
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // XỬ LÝ LẬT SPRITE (flipY)
            if (spriteRenderer != null)
            {
                if (moveInput.x < 0)
                {
                    // Di chuyển sang trái, lật Sprite để súng hướng lên trên
                    spriteRenderer.flipY = true;
                }
                else
                {
                    // Di chuyển sang phải, không lật
                    spriteRenderer.flipY = false;
                }
            }
        }
    }

    // --- HÀM MỚI: XỬ LÝ ÂM THANH ---
    void HandleMovementAudio()
    {
        // Kiểm tra xem nhân vật có đang di chuyển không
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Nếu đang di chuyển VÀ âm thanh chưa phát -> Thì bật lên
            if (!audioSource.isPlaying)
            {
                audioSource.clip = walkSound;
                audioSource.loop = true; // Cho lặp lại liên tục
                audioSource.Play();
            }
        }
        else
        {
            // Nếu đứng im VÀ âm thanh vẫn đang phát (tiếng bước chân) -> Thì tắt đi
            // Kiểm tra clip == walkSound để tránh tắt nhầm tiếng súng/chết nếu dùng chung AudioSource
            if (audioSource.isPlaying && audioSource.clip == walkSound)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }
    }

    // Hàm được Player Input gọi đến để cập nhật Input di chuyển
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    public void OnFire(InputValue value)
    {
        // Chỉ xử lý khi nút ĐƯỢC NHẤN XUỐNG (isPressed)
        if (value.isPressed)
        {
            PlayShootSound();

            // Gọi hàm bắn đạn của bạn ở đây (nếu có)
            // ShootBullet(); 
        }
    }

    void PlayShootSound()
    {
        if (shootSound != null)
        {
            // PlayOneShot: Phát đè lên âm thanh khác (vừa chạy vừa bắn vẫn nghe tiếng)
            // 0.7f là âm lượng (Volume), bạn có thể chỉnh to nhỏ tùy ý
            audioSource.PlayOneShot(shootSound, 0.7f); 
        }
    }
}