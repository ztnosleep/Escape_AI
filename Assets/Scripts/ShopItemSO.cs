using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    public string itemName; // Tên vật phẩm (vd: Súng Vip)
    public int price;       // Giá tiền
    public Sprite icon;     // Hình ảnh hiển thị
    public string itemID;   // Mã ID (vd: weapon_01) để lưu vào Cloud
}