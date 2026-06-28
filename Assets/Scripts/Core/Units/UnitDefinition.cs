using UnityEngine;

/// <summary>
/// Dữ liệu định nghĩa một loại lính có thể mua/mở khóa (data-driven).
/// Tạo asset: chuột phải trong Project > Create > Game > Unit Definition.
/// </summary>
[CreateAssetMenu(fileName = "NewUnit", menuName = "Game/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    [Header("Định danh")]
    [Tooltip("ID duy nhất, dùng làm key lưu mở khóa (vd: swordman, archer, spearman)")]
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Spawn")]
    public GameObject prefab;
    [Tooltip("Giá mua bằng VÀNG trong trận")]
    public int goldCost = 50;

    [Header("Mở khóa ở Cửa hàng (KIM CƯƠNG)")]
    [Tooltip("Mở khóa sẵn từ đầu (không cần mua ở Cửa hàng)")]
    public bool unlockedByDefault = false;
    [Tooltip("Giá mở khóa bằng KIM CƯƠNG")]
    public int diamondUnlockCost = 30;
}
