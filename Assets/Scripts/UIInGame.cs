using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // [MỚI] Thư viện cho Slider
using UnityEngine.Audio; // [MỚI] Thư viện cho Audio Mixer
using TMPro;

public class UIInGame : MonoBehaviour
{
    [Header("UI References")] 
    public GameObject pausePanel;
    public Slider sfxSlider; // [MỚI] Kéo Slider chỉnh volume vào đây
    public TextMeshProUGUI coinText;

    [Header("Audio Settings")]
    public AudioMixer audioMixer; // [MỚI] Kéo AudioMixer vào đây
    private const string MIXER_SFX = "SFXVolume"; // Tên tham số trong Mixer (xem hướng dẫn bên dưới)

    private void Start()
    {
        // [MỚI] Khi game bắt đầu, tải volume đã lưu
        // Nếu chưa lưu lần nào, mặc định là 1 (max volume)
        float savedVolume = PlayerPrefs.GetFloat("SavedSFXVolume", 1f);

        if (sfxSlider != null)
        {
            sfxSlider.value = savedVolume;
            // Cập nhật âm thanh ngay lập tức theo giá trị đã lưu
            SetSFXVolume(savedVolume);
        }
    }
    private void Update()
    {
        // [THÊM MỚI] Cập nhật số tiền liên tục
        if (coinText != null && GameManager.Instance != null)
        {
            coinText.text = GameManager.Instance.currentCoins.ToString();
        }
    }
    public void PauseGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("Ban chua keo Pause Panel vao Script!");
        }
    }
    
    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // [MỚI] Hàm này sẽ được gọi khi kéo thanh Slider
    public void SetSFXVolume(float volume)
    {
        // 1. Lưu lại vào máy (PlayerPrefs)
        PlayerPrefs.SetFloat("SavedSFXVolume", volume);

        // 2. Chỉnh âm lượng trong Mixer
        // Công thức Mathf.Log10(volume) * 20 dùng để chuyển đổi từ slider (0-1) sang Decibel (-80 đến 0)
        // Lưu ý: Slider không được để Min Value là 0, hãy để 0.0001 để tránh lỗi toán học
        if (audioMixer != null)
        {
            audioMixer.SetFloat(MIXER_SFX, Mathf.Log10(volume) * 20);
        }
    }
}