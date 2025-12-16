using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelGameManager : MonoBehaviour
{
    public static LevelGameManager Instance;

    [Header("Cài đặt Level")]
    public int currentLevelID = 1;

    [Header("Cài đặt Sao")]
    public float timeFor3Stars = 30f;
    public float timeFor2Stars = 60f;

    [Header("Cài đặt Scene")]
    public string mainMenuName = "MainMenu";

    [Header("Audio Settings")]
    public AudioClip victorySound;
    private AudioSource audioSource;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject victoryPanel;
    public GameObject[] resultStars;

    private float timer = 0;
    private bool isGameActive = true;

    // [THÊM MỚI] Key để lưu thời gian tạm thời
    private string tempTimerKey = "Temp_Current_Timer";

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        // [SỬA ĐỔI] Kiểm tra xem có thời gian đã lưu từ lần chết trước không
        if (PlayerPrefs.HasKey(tempTimerKey))
        {
            timer = PlayerPrefs.GetFloat(tempTimerKey); // Lấy lại thời gian cũ
        }
        else
        {
            timer = 0; // Nếu không có thì bắt đầu từ 0
        }
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

    // [THÊM MỚI] Hàm này gọi khi người chơi Chết, trước khi reload scene
    public void SaveTimerBeforeDeath()
    {
        PlayerPrefs.SetFloat(tempTimerKey, timer);
        PlayerPrefs.Save();
    }

    // [THÊM MỚI] Hàm này dùng để reset timer khi thoát game hoặc qua màn
    public void ResetTempTimer()
    {
        PlayerPrefs.DeleteKey(tempTimerKey);
    }

    public void CompleteLevel()
    {
        if (!isGameActive) return;

        isGameActive = false;

        // [THÊM MỚI] Hoàn thành màn chơi thì phải xóa timer tạm đi để lần sau chơi lại từ 0
        ResetTempTimer();

        if (victorySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(victorySound);
        }

        CalculateAndSaveStars();
        UnlockNextLevel();

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0;
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
        
        string starKey = "Level_" + currentLevelID + "_Stars";
        
        if (starsEarned > PlayerPrefs.GetInt(starKey, 0))
        {
            PlayerPrefs.SetInt(starKey, starsEarned);
        }
    }

    void UnlockNextLevel()
    {
        int nextLevelID = currentLevelID + 1;
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);

        if (nextLevelID > levelReached)
        {
            PlayerPrefs.SetInt("levelReached", nextLevelID);
            PlayerPrefs.Save();
            Debug.Log("Đã lưu mở khóa màn Level ID: " + nextLevelID);
        }
        if (CloudSaveManager.Instance != null)
        {
            CloudSaveManager.Instance.SaveGameData();
        }
    }

    public void NextLevelButton()
    {
        Time.timeScale = 1;
        ResetTempTimer(); // Xóa timer tạm
        
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            SceneManager.LoadScene(mainMenuName);
    }

    public void ReplayLevel()
    {
        Time.timeScale = 1;
        ResetTempTimer(); // Chơi lại từ đầu thì phải reset timer về 0
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1;
        ResetTempTimer(); // Về menu thì xóa timer tạm
        SceneManager.LoadScene(mainMenuName);
    }
}