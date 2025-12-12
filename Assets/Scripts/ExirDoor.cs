using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [Header("Cài đặt")]
    [Tooltip("Tên Scene Menu chính để quay về khi phá đảo game")]
    public string mainMenuName = "MainMenu";

    // Biến để tránh việc load scene nhiều lần nếu người chơi va chạm liên tục
    private bool levelCompleted = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra đúng là Player và chưa từng hoàn thành màn này trước đó (trong lần chạy này)
        if (other.CompareTag("Player") && !levelCompleted)
        {
            levelCompleted = true; // Đánh dấu đã thắng để không gọi code 2 lần
            WinLevelLogic();
        }
    }

    void WinLevelLogic()
    {
        // 1. Lấy Index của màn hiện tại và tính màn kế tiếp
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // 2. LƯU TIẾN ĐỘ (QUAN TRỌNG NHẤT)
        // Lấy level cao nhất đã mở khóa trước đó (mặc định là 1)
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);

        // Chỉ lưu nếu màn tiếp theo cao hơn màn đã từng mở khóa
        // (Ví dụ: Đang mở khóa tới màn 5, quay lại chơi màn 1 thì không cần lưu lại là mở màn 2)
        if (nextSceneIndex > levelReached)
        {
            PlayerPrefs.SetInt("levelReached", nextSceneIndex);
            PlayerPrefs.Save(); // Lưu ngay lập tức xuống ổ cứng
            Debug.Log("Đã mở khóa Level có Index: " + nextSceneIndex);
        }

        // 3. CHUYỂN MÀN (Kiểm tra xem còn màn nào không)
        // SceneManager.sceneCountInBuildSettings là tổng số scene trong game
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Nếu index tiếp theo vượt quá số lượng màn -> Đây là màn cuối cùng
            Debug.Log("Chúc mừng! Bạn đã phá đảo toàn bộ game!");
            
            // Quay về Menu hoặc chuyển sang Scene "EndGameCredit"
            SceneManager.LoadScene(mainMenuName);
        }
    }
}