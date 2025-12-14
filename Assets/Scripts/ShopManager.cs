using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public ShopItemSO[] shopItems; // Kéo các file Data Item1, Item2 vào đây
    public GameObject itemTemplatePrefab; // Kéo cái Prefab ô hàng vào đây
    public Transform contentTransform; // Kéo cái object "Content" trong ScrollView vào đây
    public TextMeshProUGUI coinText; // Hiển thị số tiền hiện có

    void Start()
    {
        UpdateCoinUI();
        LoadShop();
    }

    void UpdateCoinUI()
    {
        int currentCoins = PlayerPrefs.GetInt("Coins", 0); // Lấy tiền từ máy
        coinText.text = "Coins: " + currentCoins.ToString();
    }

    void LoadShop()
    {
        // Duyệt qua từng món đồ trong danh sách
        foreach (ShopItemSO item in shopItems)
        {
            // Tạo ra 1 ô hàng mới từ Prefab
            GameObject newItem = Instantiate(itemTemplatePrefab, contentTransform);
            
            // Gán thông tin hiển thị
            // Lưu ý: Bạn cần tạo script riêng cho ItemTemplate để dễ gán, nhưng ở đây mình dùng Find cho nhanh
            newItem.transform.Find("IconImage").GetComponent<Image>().sprite = item.icon;
            newItem.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text = item.price.ToString();
            newItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = item.itemName;

            // Xử lý nút Mua
            Button buyBtn = newItem.transform.Find("BuyButton").GetComponent<Button>();
            
            // Kiểm tra xem đã mua chưa (Dùng PlayerPrefs lưu trạng thái: 1 là đã mua)
            if (PlayerPrefs.GetInt("Owned_" + item.itemID, 0) == 1)
            {
                buyBtn.interactable = false;
                buyBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Đã sở hữu";
            }
            else
            {
                // Gán sự kiện bấm nút Mua
                buyBtn.onClick.AddListener(() => OnBuyClick(item, buyBtn));
            }
        }
    }

    void OnBuyClick(ShopItemSO item, Button btn)
    {
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);

        if (currentCoins >= item.price)
        {
            // 1. Trừ tiền
            currentCoins -= item.price;
            PlayerPrefs.SetInt("Coins", currentCoins);
            
            // 2. Lưu trạng thái đã mua
            PlayerPrefs.SetInt("Owned_" + item.itemID, 1);
            PlayerPrefs.Save();

            // 3. Cập nhật UI
            UpdateCoinUI();
            btn.interactable = false;
            btn.GetComponentInChildren<TextMeshProUGUI>().text = "Đã sở hữu";

            Debug.Log("Mua thành công: " + item.itemName);
            
            // --- GỌI CLOUD SAVE Ở ĐÂY NẾU CẦN ---
            // CloudSaveManager.Instance.SaveGameData();
        }
        else
        {
            Debug.Log("Không đủ tiền!");
            // Hiển thị thông báo "Bạn nghèo quá" :D
        }
    }
    
    // Nút Cheat để test tiền (Gắn vào 1 nút tạm nào đó)
    public void AddCoinCheat()
    {
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        PlayerPrefs.SetInt("Coins", currentCoins + 1000);
        UpdateCoinUI();
    }
}