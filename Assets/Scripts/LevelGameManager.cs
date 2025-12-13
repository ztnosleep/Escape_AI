using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelGameManager : MonoBehaviour
{
    public static LevelGameManager Instance;

    [Header("Cài đặt Level")]
    // --- SỬA 1: Thêm biến để tự điền số màn chơi (Ví dụ màn 1 điền số 1, màn 2 điền số 2) ---
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

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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

    public void CompleteLevel()
    {
        if (!isGameActive) return;

        isGameActive = false; 

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

        // --- SỬA 2: Dùng biến currentLevelID thay vì buildIndex ---
        // Lúc này dù Scene nằm ở index 100 thì nó vẫn lưu là Level_1_Stars nếu bạn điền số 1
        string starKey = "Level_" + currentLevelID + "_Stars";
        
        if (starsEarned > PlayerPrefs.GetInt(starKey, 0))
        {
            PlayerPrefs.SetInt(starKey, starsEarned);
        }
    }

    void UnlockNextLevel()
    {
        // Logic mở khóa màn tiếp theo dựa trên ID hiện tại + 1
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
        
        // Chuyển cảnh thì vẫn dùng Build Index để load scene tiếp theo trong danh sách
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            SceneManager.LoadScene(mainMenuName);
    }

    public void ReplayLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(mainMenuName);
    }
}