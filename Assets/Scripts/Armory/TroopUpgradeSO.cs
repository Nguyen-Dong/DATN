using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cấu hình nâng cấp (Armory) cho MỘT loại lính. Có 2 nhánh độc lập:
/// - Vũ khí (+ATK): mỗi cấp đổi visual phần vũ khí.
/// - Giáp (+DEF): mỗi cấp đổi visual tất cả phần CÒN LẠI (trừ vũ khí).
/// Mỗi cấp do designer khai báo: chỉ số + prefab visual (HeroEditor) + giá điểm kỹ năng.
/// Tạo asset: chuột phải Project > Create > Game > Troop Upgrade.
/// </summary>
[CreateAssetMenu(fileName = "NewTroopUpgrade", menuName = "Game/Troop Upgrade")]
public class TroopUpgradeSO : ScriptableObject
{
    [System.Serializable]
    public class WeaponLevel
    {
        [Tooltip("Chỉ số ATK ở cấp này")]
        public int atk;
        [Tooltip("Prefab HeroEditor để lấy visual VŨ KHÍ. Để TRỐNG ở cấp mặc định (giữ nguyên visual prefab gốc).")]
        public GameObject visualPrefab;
        [Tooltip("Giá điểm kỹ năng để LÊN cấp này. Cấp đầu tiên (mặc định) để 0.")]
        public int price;
    }

    [System.Serializable]
    public class ArmorLevel
    {
        [Tooltip("Chỉ số DEF ở cấp này")]
        public int def;
        [Tooltip("Prefab HeroEditor để lấy visual GIÁP (các phần trừ vũ khí). Để TRỐNG ở cấp mặc định.")]
        public GameObject visualPrefab;
        [Tooltip("Giá điểm kỹ năng để LÊN cấp này. Cấp đầu tiên (mặc định) để 0.")]
        public int price;
    }

    [Tooltip("Loại lính áp dụng cấu hình này")]
    public TroopType troopType;

    [Tooltip("UnitDefinition tương ứng - để Armory biết tướng đã mở khóa chưa (dùng chung hệ unlock với mua lính)")]
    public UnitDefinition unitDef;

    [Tooltip("Phần tử ĐẦU TIÊN (index 0) = Cấp 1 mặc định (price 0, visualPrefab để trống). Các phần tử sau là cấp nâng cấp.")]
    public List<WeaponLevel> weaponLevels = new List<WeaponLevel>();
    public List<ArmorLevel> armorLevels = new List<ArmorLevel>();

    /// <summary>Cấp vũ khí cao nhất có thể đạt.</summary>
    public int MaxWeaponLevel => Mathf.Max(0, weaponLevels.Count - 1);
    /// <summary>Cấp giáp cao nhất có thể đạt.</summary>
    public int MaxArmorLevel => Mathf.Max(0, armorLevels.Count - 1);

    public WeaponLevel GetWeaponLevel(int lv)
    {
        if (weaponLevels == null || weaponLevels.Count == 0) return null;
        return weaponLevels[Mathf.Clamp(lv, 0, weaponLevels.Count - 1)];
    }

    public ArmorLevel GetArmorLevel(int lv)
    {
        if (armorLevels == null || armorLevels.Count == 0) return null;
        return armorLevels[Mathf.Clamp(lv, 0, armorLevels.Count - 1)];
    }
}
