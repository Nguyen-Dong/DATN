using UnityEngine;

/// <summary>
/// Lưu trạng thái MỞ KHÓA loại lính (meta, persist bằng PlayerPrefs).
/// Lính có thể mua trong trận (bằng vàng) chỉ khi đã được mở khóa.
/// </summary>
public static class TroopUnlockStore
{
    private const string PREFIX = "TroopUnlocked_";

    public static bool IsUnlocked(UnitDefinition def)
    {
        if (def == null) return false;
        if (def.unlockedByDefault) return true;
        return PlayerPrefs.GetInt(PREFIX + def.id, 0) == 1;
    }

    public static void Unlock(UnitDefinition def)
    {
        if (def == null) return;
        PlayerPrefs.SetInt(PREFIX + def.id, 1);
        PlayerPrefs.Save();
    }

    /// <summary>Khóa lại (chủ yếu để test/reset).</summary>
    public static void Lock(UnitDefinition def)
    {
        if (def == null) return;
        PlayerPrefs.DeleteKey(PREFIX + def.id);
        PlayerPrefs.Save();
    }
}
