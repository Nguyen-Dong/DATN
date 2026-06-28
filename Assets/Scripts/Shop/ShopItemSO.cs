using UnityEngine;

/// <summary>
/// 3 loại chiêu (lính) bán trong Cửa hàng. Dùng làm khóa phân biệt item.
/// </summary>
public enum TroopType
{
    Sword,  // Kiếm
    Bow,    // Cung
    Spear   // Giáo
}

/// <summary>
/// Định nghĩa một vật phẩm trong Cửa hàng (mua bằng KIM CƯƠNG để mở khóa loại lính).
/// Tạo asset: chuột phải trong Project > Create > Game > Shop Item.
/// </summary>
[CreateAssetMenu(fileName = "NewShopItem", menuName = "Game/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    [Tooltip("Loại chiêu (kiếm / cung / giáo) - dùng để phân biệt và làm khóa mở khóa")]
    public TroopType troopType;

    [Tooltip("Tên hiển thị trên Cửa hàng")]
    public string displayName;

    [Tooltip("Mô tả ngắn về chiêu")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("Giá mua bằng KIM CƯƠNG")]
    public int price;

    [Tooltip("Ảnh icon hiển thị trên Cửa hàng (UI skill)")]
    public Sprite icon;
}
