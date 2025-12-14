using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Tiền trong game
    public int currentCoins = 0;

    // Các chỉ số cấp độ hiện tại (Mặc định lv 1)
    public int healthLevel = 1;
    public int damageLevel = 1;
    public int speedLevel = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ script này sống qua các Scene
            LoadData(); // Tải dữ liệu khi vào game
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Hàm cộng tiền (Gọi khi giết zombie)
    public void AddCoin(int amount)
    {
        currentCoins += amount;
        // Có thể thêm UI update tiền ở đây
    }

    // Hàm lưu dữ liệu (PlayerPrefs)
    public void SaveData()
    {
        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.SetInt("HealthLevel", healthLevel);
        PlayerPrefs.SetInt("DamageLevel", damageLevel);
        PlayerPrefs.SetInt("SpeedLevel", speedLevel);
        PlayerPrefs.Save();
    }

    // Hàm tải dữ liệu
    public void LoadData()
    {
        currentCoins = PlayerPrefs.GetInt("Coins", 0); // Mặc định 0 xu
        healthLevel = PlayerPrefs.GetInt("HealthLevel", 1);
        damageLevel = PlayerPrefs.GetInt("DamageLevel", 1);
        speedLevel = PlayerPrefs.GetInt("SpeedLevel", 1);
    }
}