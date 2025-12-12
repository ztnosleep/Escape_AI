using UnityEngine;
using UnityEngine.SceneManagement; // Cần để tải lại màn chơi

public class GuardVision : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("BỊ BẮT RỒI!");


            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    }
}