using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Bắt buộc có dòng này để dùng Coroutine

public class GuardVision : MonoBehaviour
{
    [Header("Cài đặt Âm thanh")]
    public AudioClip alarmSound; // Kéo file tiếng còi hú vào đây
    private AudioSource audioSource;

    private bool isDetected = false; // Biến cờ để tránh code chạy 2 lần liên tục

    void Start()
    {
        // Tự động lấy hoặc thêm AudioSource nếu quên
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra đúng là Player và chưa từng bị phát hiện trước đó
        if (other.CompareTag("Player") && !isDetected)
        {
            isDetected = true; // Đánh dấu đã bị bắt
            // Debug.Log("BỊ BẮT RỒI!");

            // Gọi quy trình: Hú còi -> Chờ -> Reset
            StartCoroutine(PlayAlarmThenRestart());
        }
    }

    // Coroutine giúp xử lý tuần tự theo thời gian
    IEnumerator PlayAlarmThenRestart()
    {
        // 1. Phát tiếng báo động
        if (alarmSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(alarmSound);
        }

        // (Tùy chọn) Tại đây bạn có thể code để Player dừng di chuyển (Disable Script PlayerMovement)
        // if (playerMovement != null) playerMovement.enabled = false;

        // 2. Chờ khoảng 1 giây (hoặc bằng độ dài file âm thanh) để người chơi nghe thấy tiếng kêu
        yield return new WaitForSeconds(2f); 

        // 3. Sau khi chờ xong mới tải lại màn chơi
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}