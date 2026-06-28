using UnityEngine;

/// <summary>
/// Lưu CẤP nâng cấp Armory (vũ khí + giáp) theo từng loại lính, persist bằng PlayerPrefs.
/// Cấp mặc định = 0. Là nguồn dữ liệu chung cho cả UI Armory lẫn việc áp cấp khi spawn lính.
/// </summary>
public static class ArmoryStore
{
    private const string KEY_WEAPON = "Armory_Wpn_";
    private const string KEY_ARMOR = "Armory_Arm_";

    public static int GetWeaponLevel(TroopType type) => PlayerPrefs.GetInt(KEY_WEAPON + type, 0);
    public static int GetArmorLevel(TroopType type) => PlayerPrefs.GetInt(KEY_ARMOR + type, 0);

    public static void SetWeaponLevel(TroopType type, int level)
    {
        PlayerPrefs.SetInt(KEY_WEAPON + type, Mathf.Max(0, level));
        PlayerPrefs.Save();
    }

    public static void SetArmorLevel(TroopType type, int level)
    {
        PlayerPrefs.SetInt(KEY_ARMOR + type, Mathf.Max(0, level));
        PlayerPrefs.Save();
    }
}
