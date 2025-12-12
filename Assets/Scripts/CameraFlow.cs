using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Kéo thả object Player vào đây
    public float smoothTime = 0.3f; // Thời gian để camera "bắt kịp" Player (càng nhỏ càng nhanh)
    
    // Đặt Z = -10 vì camera 2D luôn phải ở -10
    private Vector3 offset = new Vector3(0f, 0f, -10f); 
    
    // Biến này cần cho hàm SmoothDamp (không cần chỉnh)
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target != null)
        {
            // Tính toán vị trí mong muốn
            Vector3 targetPosition = target.position + offset;
            
            // Dùng SmoothDamp để di chuyển camera mượt mà
            transform.position = Vector3.SmoothDamp(
                transform.position, // Vị trí hiện tại
                targetPosition,     // Vị trí muốn đến
                ref velocity,       // Biến vận tốc (để hàm tự quản lý)
                smoothTime          // Thời gian di chuyển
            );
        }
    }
}