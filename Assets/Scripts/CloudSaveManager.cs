using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        // 1. Khởi động Unity Services
        await UnityServices.InitializeAsync();

        // 2. Đăng nhập ẩn danh (Anonymous)
        // Trên mobile, nó sẽ tự tạo ID dựa trên thiết bị
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            // Debug.Log("Đã đăng nhập Cloud với ID: " + AuthenticationService.Instance.PlayerId);
        }
    }

    // --- HÀM LƯU DỮ LIỆU ---
    public async void SaveGameData()
    {
        // Lấy dữ liệu từ PlayerPrefs (hoặc biến game) ra để chuẩn bị gửi
        var dataToSave = new Dictionary<string, object>
        {
            { "coins", PlayerPrefs.GetInt("Coins", 0) },
            { "levelReached", PlayerPrefs.GetInt("levelReached", 1) }
            // Bạn có thể thêm các key khác vào đây
        };

        try
        {
            // Gửi lên Cloud
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log("Đã lưu dữ liệu lên Cloud thành công!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi lưu Cloud: " + e.Message);
        }
    }

    // --- HÀM TẢI DỮ LIỆU (Đồng bộ từ Cloud về máy) ---
    public async void LoadGameData()
    {
        try
        {
            // Tải dữ liệu về
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "coins", "levelReached" });

            // Kiểm tra và lưu ngược lại vào PlayerPrefs của máy
            if (savedData.TryGetValue("coins", out var coinsItem))
            {
                int coinVal = coinsItem.Value.GetAs<int>();
                PlayerPrefs.SetInt("Coins", coinVal);
                Debug.Log("Đã tải Coins: " + coinVal);
            }

            if (savedData.TryGetValue("levelReached", out var levelItem))
            {
                int levelVal = levelItem.Value.GetAs<int>();
                PlayerPrefs.SetInt("levelReached", levelVal);
                Debug.Log("Đã tải Level: " + levelVal);
            }

            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi tải Cloud: " + e.Message);
        }
    }
}