using UnityEngine;
using TMPro; // Dùng TextMeshPro
using UnityEngine.UI;

public class UpgradeShopController : MonoBehaviour
{
    [Header("Player References")]
    public PlayerHealth playerHealth;
    public PlayerMovement playerMovement;
    public PlayerCombat playerCombat;

    [Header("UI References")]
    public TextMeshProUGUI coinText; // Hiển thị tiền hiện có
    
    // UI cho Nâng cấp Máu
    public TextMeshProUGUI healthCostText;
    public TextMeshProUGUI healthLevelText;
    public int healthBaseCost = 10; // Giá khởi điểm

    // UI cho Nâng cấp Damage
    public TextMeshProUGUI damageCostText;
    public TextMeshProUGUI damageLevelText;
    public int damageBaseCost = 15;

    // UI cho Nâng cấp Tốc chạy
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI speedLevelText;
    public int speedBaseCost = 10;

    void Start()
    {
        // Tự tìm Player nếu chưa kéo vào
        if(playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if(playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();
        if(playerCombat == null) playerCombat = FindFirstObjectByType<PlayerCombat>();

        UpdateUI();
    }

    void Update()
    {
        // Cập nhật hiển thị tiền liên tục (hoặc tối ưu hơn thì chỉ update khi thay đổi)
        if(GameManager.Instance != null)
            coinText.text = "Coins: " + GameManager.Instance.currentCoins.ToString();
    }

    // --- BUTTON: MUA NÂNG CẤP MÁU ---
    public void BuyHealthUpgrade()
    {
        int currentLvl = GameManager.Instance.healthLevel;
        int cost = healthBaseCost * currentLvl; // Giá tăng theo cấp (vd: 10, 20, 30...)

        if (GameManager.Instance.currentCoins >= cost)
        {
            // 1. Trừ tiền
            GameManager.Instance.currentCoins -= cost;
            
            // 2. Tăng cấp trong data
            GameManager.Instance.healthLevel++;

            // 3. Tác động ngay lên Player (nếu đang chơi)
            if(playerHealth != null) playerHealth.IncreaseMaxHealth(1); // Tăng 1 máu tối đa

            // 4. Lưu và Update UI
            GameManager.Instance.SaveData();
            UpdateUI();
        }
        else
        {
            Debug.Log("Không đủ tiền!");
        }
    }

    // --- BUTTON: MUA NÂNG CẤP DAMAGE ---
    public void BuyDamageUpgrade()
    {
        int currentLvl = GameManager.Instance.damageLevel;
        int cost = damageBaseCost * currentLvl; 

        if (GameManager.Instance.currentCoins >= cost)
        {
            GameManager.Instance.currentCoins -= cost;
            GameManager.Instance.damageLevel++;

            if(playerCombat != null) playerCombat.UpgradeDamage(1); // Tăng 1 Damage

            GameManager.Instance.SaveData();
            UpdateUI();
        }
    }

    // --- BUTTON: MUA NÂNG CẤP TỐC ĐỘ ---
    public void BuySpeedUpgrade()
    {
        int currentLvl = GameManager.Instance.speedLevel;
        int cost = speedBaseCost * currentLvl;

        if (GameManager.Instance.currentCoins >= cost)
        {
            GameManager.Instance.currentCoins -= cost;
            GameManager.Instance.speedLevel++;

            if(playerMovement != null) playerMovement.UpgradeBaseSpeed(0.5f); // Tăng 0.5 tốc độ

            GameManager.Instance.SaveData();
            UpdateUI();
        }
    }

    // Hàm cập nhật chữ trên các nút
    void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        // Health UI
        healthLevelText.text = "Level: " + GameManager.Instance.healthLevel;
        healthCostText.text = "Cost: " + (healthBaseCost * GameManager.Instance.healthLevel);

        // Damage UI
        damageLevelText.text = "Level: " + GameManager.Instance.damageLevel;
        damageCostText.text = "Cost: " + (damageBaseCost * GameManager.Instance.damageLevel);

        // Speed UI
        speedLevelText.text = "Level: " + GameManager.Instance.speedLevel;
        speedCostText.text = "Cost: " + (speedBaseCost * GameManager.Instance.speedLevel);
    }
}