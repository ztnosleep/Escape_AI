using UnityEngine;
using UnityEngine.UI; // Để dùng Button
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    public Button[] levelButtons; // Mảng chứa tất cả các nút Level

    void Start()
    {
        // Lấy level cao nhất đã chơi được từ bộ nhớ (mặc định là 1)
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);

        // Duyệt qua tất cả các nút
        for (int i = 0; i < levelButtons.Length; i++)
        {
            // Nếu vị trí nút (i) lớn hơn level đã đạt được -> Khóa lại
            // Ví dụ: levelReached = 1. i = 0 (nút 1) -> Mở. i = 1 (nút 2) -> Khóa.
            if (i + 1 > levelReached)
            {
                levelButtons[i].interactable = false; // Không cho bấm
                // (Tùy chọn) Đổi màu nút sang xám để người chơi biết bị khóa
                // levelButtons[i].image.color = Color.gray; 
            }
        }
    }

    // Hàm này sẽ được gắn vào từng nút
    public void LoadLevel(int levelIndex)
    {
        // Chuyển sang scene có tên: "Level_1", "Level_2"...
        string levelName = "Level_" + levelIndex; 
        SceneManager.LoadScene(levelName);
    }
    
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}