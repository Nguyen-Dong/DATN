using System.Collections.Generic;

/// <summary>
/// Sổ đăng ký các đơn vị (UnitType) theo loại lính. Cho phép truy vấn nhanh:
/// đếm / lấy danh sách đơn vị CÒN SỐNG của một loại (dùng cho hệ chiêu trong trận).
/// Vì chỉ lính Player có UnitType nên registry này thực chất là các đơn vị phe Player.
/// </summary>
public static class UnitRegistry
{
    private static readonly Dictionary<TroopType, List<UnitType>> map = new Dictionary<TroopType, List<UnitType>>();

    public static void Register(UnitType u)
    {
        if (u == null) return;
        if (!map.TryGetValue(u.type, out List<UnitType> list))
        {
            list = new List<UnitType>();
            map[u.type] = list;
        }
        if (!list.Contains(u)) list.Add(u);
    }

    public static void Unregister(UnitType u)
    {
        if (u == null) return;
        if (map.TryGetValue(u.type, out List<UnitType> list))
            list.Remove(u);
    }

    public static int CountAlive(TroopType type)
    {
        int count = 0;
        if (map.TryGetValue(type, out List<UnitType> list))
            foreach (UnitType u in list)
                if (u != null && u.IsAlive) count++;
        return count;
    }

    public static List<UnitType> GetAlive(TroopType type)
    {
        var result = new List<UnitType>();
        if (map.TryGetValue(type, out List<UnitType> list))
            foreach (UnitType u in list)
                if (u != null && u.IsAlive) result.Add(u);
        return result;
    }
}
