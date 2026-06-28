using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Danh sách tất cả loại lính trong game (data-driven). Dùng cho UI Cửa hàng / mua lính.
/// Tạo asset: chuột phải trong Project > Create > Game > Unit Roster.
/// </summary>
[CreateAssetMenu(fileName = "UnitRoster", menuName = "Game/Unit Roster")]
public class UnitRoster : ScriptableObject
{
    public List<UnitDefinition> units = new List<UnitDefinition>();

    public UnitDefinition GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (UnitDefinition u in units)
            if (u != null && u.id == id) return u;
        return null;
    }
}
