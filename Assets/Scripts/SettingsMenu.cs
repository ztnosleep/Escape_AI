using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer mainMixer; // Kéo AudioMixer vào đây
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        // Lấy giá trị đã lưu lần trước (nếu có), mặc định là 0 (Max)
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0);

        // Cập nhật lên Slider
        musicSlider.value = savedMusic;
        sfxSlider.value = savedSFX;

        // Áp dụng âm thanh luôn
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);
    }

    // Gắn hàm này vào sự kiện "On Value Changed" của Music Slider
    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume); // Lưu lại
    }

    // Gắn hàm này vào sự kiện "On Value Changed" của SFX Slider
    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("SFXVolume", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume); // Lưu lại
    }
    
    public void CloseSettings()
    {
        gameObject.SetActive(false); // Tắt bảng
        PlayerPrefs.Save();
    }
}