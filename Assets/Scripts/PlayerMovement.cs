using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; 
    private Vector2 moveInput; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        
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

    // Hàm được Player Input gọi đến để cập nhật Input di chuyển
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}