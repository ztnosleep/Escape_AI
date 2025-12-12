using UnityEngine;

public class GuardPatrol : MonoBehaviour
{
    public Transform patrolPointA; // Kéo thả điểm A vào đây
    public Transform patrolPointB; // Kéo thả điểm B vào đây
    public float patrolSpeed = 2f;

    private Transform currentTarget;

    void Start()
    {
        // Ban đầu, đi về phía điểm B
        currentTarget = patrolPointB;
    }

    void Update()
    {
        // Di chuyển lính canh đến mục tiêu
        transform.position = Vector2.MoveTowards(
            transform.position, 
            currentTarget.position, 
            patrolSpeed * Time.deltaTime
        );

        // Kiểm tra xem đã đến gần mục tiêu chưa
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            // Nếu mục tiêu đang là B, thì đổi sang A
            if (currentTarget == patrolPointB)
            {
                currentTarget = patrolPointA;
            }
            // Nếu mục tiêu đang là A, thì đổi sang B
            else
            {
                currentTarget = patrolPointB;
            }
        }

        // (Nâng cao) Làm lính canh xoay mặt theo hướng đi
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        if (direction != Vector2.zero) // Tránh báo lỗi khi đứng yên
        {
            // Phép toán để xoay sprite 2D (trừ 90 độ nếu sprite của bạn mặc định nhìn lên trên)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }
}