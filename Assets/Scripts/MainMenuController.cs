using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panel Cửa Hàng & Cài Đặt")]
    public GameObject mainMenuPanel; 
    public GameObject shopPanel;    
    public GameObject settingsPanel; 

    [Header("Audio Settings")]
    public AudioMixer mainMixer; 
    public Slider musicSlider;
    public Slider sfxSlider;

    // Tên tham số phải giống hệt bên UIInGame và trong AudioMixer
    private const string MIXER_MUSIC = "MusicVolume"; // Bạn cần Expose tham số này trong Mixer
    private const string MIXER_SFX = "SFXVolume";     // Đã khớp với UIInGame

    private void Start()
    {
        // --- [QUAN TRỌNG] Tải lại volume đã lưu khi mở game ---
    
        // 1. Tải Music
        float savedMusic = PlayerPrefs.GetFloat("SavedMusicVolume", 1f);
        Debug.Log("Volume đã lưu: " + savedMusic);
        if (musicSlider != null)
    {
        Debug.Log("Đã tìm thấy Slider, đang cập nhật vị trí..."); // Kiểm tra dòng này
        musicSlider.value = savedMusic;
        SetMusicVolume(savedMusic);
    }
    else
    {
        Debug.LogError("CHƯA KÉO SLIDER VÀO SCRIPT KÌA BẠN ƠI!"); // Nếu dòng này hiện ra thì làm theo bước 1
    }

        // 2. Tải SFX (Dùng chung key "SavedSFXVolume" với UIInGame để đồng bộ)
        float savedSFX = PlayerPrefs.GetFloat("SavedSFXVolume", 1f);
        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFX;
            SetSFXVolume(savedSFX);
        }
    }

    // --- PHẦN 1: CHUYỂN SCENE ---
    public void PlayGame()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    // --- PHẦN 2: BẬT TẮT PANEL ---
    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        shopPanel.SetActive(false);
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Đã thoát game!");
    }

    // --- PHẦN 3: AUDIO (Đã sửa đổi để đồng bộ) ---

    public void SetMusicVolume(float sliderValue)
    {
        // 1. Lưu lại giá trị (Để lần sau mở game còn nhớ)
        PlayerPrefs.SetFloat("SavedMusicVolume", sliderValue);

        // 2. Chỉnh Mixer
        float volume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20; 
        if (sliderValue <= 0.001f) volume = -80f; // Tắt hẳn nếu kéo về 0

        mainMixer.SetFloat(MIXER_MUSIC, volume);
    }

    public void SetSFXVolume(float sliderValue)
    {
        // 1. Lưu lại giá trị (Dùng chung key với UIInGame)
        PlayerPrefs.SetFloat("SavedSFXVolume", sliderValue);

        // 2. Chỉnh Mixer (Dùng tên tham số MIXER_SFX = "SFXVolume")
        float volume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
        if (sliderValue <= 0.001f) volume = -80f;

        mainMixer.SetFloat(MIXER_SFX, volume);
    }
}