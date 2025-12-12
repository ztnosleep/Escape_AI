using UnityEngine;
using UnityEngine.UI;

public class LevelStarUI : MonoBehaviour
{
    public int levelIndex; // Điền số màn chơi vào đây (1, 2, 3...)
    public GameObject[] stars; // Kéo 3 cái hình Star1, Star2, Star3 vào đây

    void Start()
    {
        UpdateStarDisplay();
    }

    public void UpdateStarDisplay()
    {
        // Lấy số sao đã lưu từ PlayerPrefs
        // Key phải trùng khớp với bên LevelGameManager: "Level_" + index + "_Stars"
        string saveKey = "Level_" + levelIndex + "_Stars";
        int starsEarned = PlayerPrefs.GetInt(saveKey, 0);

        // Vòng lặp bật/tắt sao
        for (int i = 0; i < stars.Length; i++)
        {
            // Logic: Nếu sao kiếm được là 2, thì i=0 (sao 1) hiện, i=1 (sao 2) hiện, i=2 (sao 3) ẩn.
            if (i < starsEarned)
            {
                stars[i].SetActive(true); // Hoặc đổi màu: stars[i].color = Color.yellow;
            }
            else
            {
                stars[i].SetActive(false); // Hoặc đổi màu: stars[i].color = Color.gray;
            }
        }
    }
}