using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour
{
    public static MusicController instance; // Biến Singleton

    void Awake()
    {
        // 1. Xử lý Trùng lặp (Singleton)
        // Nếu đã có 1 cái loa nhạc rồi, mà tạo thêm cái nữa -> Hủy cái mới đi
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Nếu chưa có -> Đây là cái loa gốc -> Giữ nó lại
        instance = this;
        DontDestroyOnLoad(gameObject); // Lệnh thần thánh: Không hủy khi chuyển Scene
    }

    void Update()
    {
        // 2. Kiểm tra xem đang ở Scene nào để quyết định có tắt nhạc không
        string currentScene = SceneManager.GetActiveScene().name;

        // Logic: Chỉ cho phép nhạc tồn tại ở "MainMenu" và "LevelSelect"
        // Nếu tên Scene KHÔNG PHẢI là 2 cái trên -> Tức là đã vào game -> Hủy loa
        if (currentScene != "MainMenu" && currentScene != "LevelSelect")
        {
            Destroy(gameObject);
        }
    }
    
    // Khi bị hủy thì reset biến instance về null để lần sau quay lại Menu nó tạo lại cái mới
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}