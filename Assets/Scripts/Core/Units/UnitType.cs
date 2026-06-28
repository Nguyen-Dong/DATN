using UnityEngine;

/// <summary>
/// Gắn lên prefab gốc của lính để khai báo nó thuộc LOẠI nào (kiếm/cung/giáo).
/// Dùng cho Armory (áp đúng cấp đã lưu) và hệ chiêu trong trận (tìm đơn vị theo loại).
/// Tự đăng ký vào <see cref="UnitRegistry"/> để truy vấn nhanh số đơn vị còn sống theo loại.
/// </summary>
public class UnitType : MonoBehaviour
{
    public TroopType type;

    private Entity entity;

    private void Awake()
    {
        entity = GetComponent<Entity>();
    }

    private void OnEnable() => UnitRegistry.Register(this);
    private void OnDisable() => UnitRegistry.Unregister(this);

    /// <summary>Còn sống (chưa chết).</summary>
    public bool IsAlive => entity == null || !entity.dead;
}
