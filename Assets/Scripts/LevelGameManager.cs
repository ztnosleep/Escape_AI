using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelGameManager : MonoBehaviour
{
    public static LevelGameManager Instance;

    [Header("Cài đặt Sao")]
    public float timeFor3Stars = 30f;
    public float timeFor2Stars = 60f;

    [Header("Cài đặt Scene")]
    public string mainMenuName = "MainMenu"; // Tên scene Menu chính

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject victoryPanel;
    public GameObject[] resultStars;

    private float timer = 0;
    private bool isGameActive = true;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isGameActive)
        {
            timer += Time.deltaTime;
            if (timerText != null)
            {
                float minutes = Mathf.FloorToInt(timer / 60);
                float seconds = Mathf.FloorToInt(timer % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    public void CompleteLevel()
    {
        if (!isGameActive) return;

        isGameActive = false; 
        
        CalculateAndSaveStars();
        UnlockNextLevel();

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0; // Dừng game lại
        }
    }

    void CalculateAndSaveStars()
    {
        int starsEarned = 1;
        if (timer <= timeFor3Stars) starsEarned = 3;
        else if (timer <= timeFor2Stars) starsEarned = 2;

        for (int i = 0; i < resultStars.Length; i++)
        {
            if (i < starsEarned) resultStars[i].SetActive(true);
            else resultStars[i].SetActive(false);
        }

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        string starKey = "Level_" + currentLevelIndex + "_Stars";
        if (starsEarned > PlayerPrefs.GetInt(starKey, 0))
        {
            PlayerPrefs.SetInt(starKey, starsEarned);
        }
    }

    void UnlockNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);

        if (nextSceneIndex > levelReached)
        {
            PlayerPrefs.SetInt("levelReached", nextSceneIndex);
            PlayerPrefs.Save();
            Debug.Log("Đã lưu mở khóa màn: " + nextSceneIndex);
        }
    }

    // --- CÁC HÀM NÚT BẤM (ĐÃ BỔ SUNG) ---

    // 1. Nút Tiếp Tục (Next Level)
    public void NextLevelButton()
    {
        Time.timeScale = 1; // Mở lại thời gian
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            SceneManager.LoadScene(mainMenuName);
    }

    // 2. Nút Chơi Lại (Replay) - MỚI
    public void ReplayLevel()
    {
        Time.timeScale = 1; // Quan trọng: Phải mở lại thời gian trước khi load
        // Load lại chính Scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 3. Nút Về Menu (Back To Menu) - MỚI
    public void BackToMenu()
    {
        Time.timeScale = 1; // Quan trọng: Phải mở lại thời gian
        SceneManager.LoadScene(mainMenuName);
    }
}