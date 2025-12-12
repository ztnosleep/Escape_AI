using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private bool levelCompleted = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !levelCompleted)
        {
            levelCompleted = true;
            
            // Chỉ cần gọi Manager xử lý thôi
            if (LevelGameManager.Instance != null)
            {
                LevelGameManager.Instance.CompleteLevel();
            }
            else
            {
                Debug.LogError("Chưa bỏ LevelGameManager vào Scene kìa!");
            }
        }
    }
}