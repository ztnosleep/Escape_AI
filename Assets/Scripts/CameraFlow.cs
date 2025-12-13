using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;           // Player
    public float smoothTime = 0.3f;
    public BoxCollider2D mapBounds;    // Kéo MapBounds vào đây

    private Vector3 offset = new Vector3(0f, 0f, -10f);
    private Vector3 velocity = Vector3.zero;

    private float minX, maxX, minY, maxY;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        Bounds bounds = mapBounds.bounds;

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        minX = bounds.min.x + camHalfWidth;
        maxX = bounds.max.x - camHalfWidth;
        minY = bounds.min.y + camHalfHeight;
        maxY = bounds.max.y - camHalfHeight;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        Vector3 smoothPos = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        // 🚧 Giới hạn camera trong map
        smoothPos.x = Mathf.Clamp(smoothPos.x, minX, maxX);
        smoothPos.y = Mathf.Clamp(smoothPos.y, minY, maxY);

        transform.position = smoothPos;
    }
}
