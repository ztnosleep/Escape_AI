using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingScreen; // Panel Loading
    public Slider slider;            // Thanh trượt
    public Text progressText;        // Text %

    // SỬA ĐỔI: Nhận vào số Level (1, 2, 3...) thay vì Index trong Build Settings
    public void LoadLevel (int levelIndex)
    {
        // Tạo tên Scene theo công thức: "Level_" + số
        // Ví dụ: LoadLevel(1) -> sẽ tìm scene tên "Level_1"
        string levelName = "Level_" + levelIndex;

        // Gọi Coroutine với tên Scene vừa tạo
        StartCoroutine(LoadAsynchronously(levelName));
    }

    // SỬA ĐỔI: Nhận vào string (tên Scene) thay vì int
    IEnumerator LoadAsynchronously (string sceneName)
    {
        // 1. QUAN TRỌNG NHẤT: Bật màn hình Loading lên TRƯỚC TIÊN
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        // 2. Bắt đầu tải Scene ngầm bằng TÊN SCENE
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        // Ngăn không cho Scene tự bật lên ngay (để chờ thanh loading chạy)
        operation.allowSceneActivation = false;

        float fakeProgress = 0f;

        // 3. Vòng lặp cập nhật thanh trượt
        while (!operation.isDone)
        {
            // Tiến trình thực (0 -> 0.9)
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Tiến trình giả (Tăng dần theo thời gian để tạo hiệu ứng)
            fakeProgress += Time.deltaTime * 0.5f; // Chỉnh 0.5f thành số nhỏ hơn nếu muốn tải lâu hơn

            // Hiển thị cái nào nhỏ hơn (để thanh không bị nhảy cóc)
            float displayProgress = Mathf.Min(fakeProgress, realProgress);

            // Cập nhật UI
            if (slider != null) slider.value = displayProgress;
            if (progressText != null) progressText.text = (displayProgress * 100f).ToString("F0") + "%";

            // Logic kiểm tra điều kiện hoàn thành
            // Nếu thanh giả đã chạy xong (>= 1) VÀ game đã tải xong (real >= 1)
            if (fakeProgress >= 1f && realProgress >= 1f)
            {
                // Cho phép vào game
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}