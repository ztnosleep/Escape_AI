using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; 

public class GuardVision : MonoBehaviour
{
    [Header("Cài đặt Âm thanh")]
    public AudioClip alarmSound; 
    private AudioSource audioSource;

    private bool isDetected = false; 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDetected)
        {
            isDetected = true; 
            StartCoroutine(PlayAlarmThenRestart());
        }
    }

    IEnumerator PlayAlarmThenRestart()
    {
        // 1. Phát tiếng báo động
        if (alarmSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(alarmSound);
        }

        // 2. Chờ 2 giây
        yield return new WaitForSeconds(2f); 

        // [THÊM MỚI] 3. Lưu thời gian hiện tại trước khi reset
        // Gọi hàm SaveTimerBeforeDeath() mà chúng ta vừa viết bên LevelGameManager
        if (LevelGameManager.Instance != null)
        {
            LevelGameManager.Instance.SaveTimerBeforeDeath();
        }

        // 4. Tải lại màn chơi
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}