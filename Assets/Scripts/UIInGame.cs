using UnityEngine;
using UnityEngine.SceneManagement;


public class UIInGame : MonoBehaviour
{
    [Header("Keo Panel Vao Day")] 
    public GameObject pausePanel; 

    public void PauseGame()
    {
        if (pausePanel != null) // Kiểm tra an toàn
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
        SceneManager.LoadScene("LevelSelect");
    }
}