using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý 3 nút chiêu trong trận (Kiếm/Cung/Giáo).
/// - Nút chỉ bấm được khi: còn số lượng chiêu (ShopManager.GetCount > 0) VÀ còn ít nhất 1 đơn vị loại đó còn sống.
/// - Kiếm: buff +ATK; Giáo: buff +DEF (10s) cho TẤT CẢ đơn vị loại đó + bật VFX.
/// - Cung: bắn 1 lúc nhiều mũi tên rơi quanh vị trí mục tiêu (cụm địch gần nhất).
/// Mỗi lần dùng trừ 1 số lượng chiêu (ShopManager.TryConsume).
/// </summary>
public class BattleSkillManager : MonoBehaviour
{
    [Serializable]
    public class SkillSlot
    {
        public TroopType type;
        public Button button;
        [Tooltip("Text hiển thị số lượng chiêu còn lại (tùy chọn)")]
        public TMP_Text countText;
        [Tooltip("UnitDefinition của loại lính này - để khóa skill khi tướng chưa mở khóa")]
        public UnitDefinition unitDef;
    }

    [SerializeField] private List<SkillSlot> slots = new List<SkillSlot>();

    [Header("Buff (Kiếm = +ATK, Giáo = +DEF)")]
    [SerializeField] private float buffDuration = 10f;
    [SerializeField] private float swordAtkBonus = 5f;
    [SerializeField] private float spearDefBonus = 5f;
    [SerializeField] private GameObject swordVfxPrefab;
    [SerializeField] private GameObject spearVfxPrefab;

    [Header("Cung (1 cung thủ bắn loạt)")]
    [Tooltip("Số mũi tên bắn 1 lúc")]
    [SerializeField] private int arrowCount = 10;
    [Tooltip("Bán kính rải tên quanh mục tiêu (để loạt tên tản ra)")]
    [SerializeField] private float spreadRadius = 2.5f;
    [Tooltip("VFX hiển thị tại mục tiêu (tùy chọn)")]
    [SerializeField] private GameObject bowTargetVfxPrefab;

    private void Start()
    {
        foreach (SkillSlot s in slots)
        {
            if (s == null || s.button == null) continue;
            TroopType t = s.type;
            s.button.onClick.AddListener(() => UseSkill(t));
        }
    }

    private void Update()
    {
        // Cập nhật trạng thái bấm được + số lượng cho từng nút
        foreach (SkillSlot s in slots)
        {
            if (s == null || s.button == null) continue;
            int count = ShopManager.GetCount(s.type);
            bool hasUnit = UnitRegistry.CountAlive(s.type) > 0;
            bool unlocked = s.unitDef == null || TroopUnlockStore.IsUnlocked(s.unitDef);
            s.button.interactable = unlocked && count > 0 && hasUnit;
            if (s.countText != null) s.countText.text = count.ToString();
        }
    }

    private void UseSkill(TroopType type)
    {
        // Chặn an toàn (dù nút đã gate)
        if (ShopManager.GetCount(type) <= 0) return;
        if (UnitRegistry.CountAlive(type) <= 0) return;
        if (!ShopManager.TryConsume(type, 1)) return;

        switch (type)
        {
            case TroopType.Sword: ApplyBuff(type, swordAtkBonus, 0f, swordVfxPrefab); break;
            case TroopType.Spear: ApplyBuff(type, 0f, spearDefBonus, spearVfxPrefab); break;
            case TroopType.Bow: ArcherVolley(); break;
        }
    }

    private void ApplyBuff(TroopType type, float atk, float def, GameObject vfx)
    {
        foreach (UnitType u in UnitRegistry.GetAlive(type))
        {
            SkillBuff buff = u.gameObject.AddComponent<SkillBuff>();
            buff.Begin(atk, def, buffDuration, vfx);
        }
    }

    /// <summary>
    /// 1 cung thủ (gần mục tiêu nhất) bắn 1 loạt arrowCount mũi tên cùng lúc, rải quanh mục tiêu.
    /// Bắn bằng cơ chế thường của cung thủ (ballistic), chỉ 1 lần/lượt dùng.
    /// </summary>
    private void ArcherVolley()
    {
        List<UnitType> bows = UnitRegistry.GetAlive(TroopType.Bow);
        if (bows.Count == 0) return;

        Vector3 target = GetArrowTarget();

        // Chọn cung thủ gần mục tiêu nhất để bắn loạt
        ArcherAttack shooter = null;
        float bestSqr = float.MaxValue;
        foreach (UnitType u in bows)
        {
            ArcherAttack aa = u.GetComponentInChildren<ArcherAttack>();
            if (aa == null) continue;
            float sqr = ((Vector2)u.transform.position - (Vector2)target).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; shooter = aa; }
        }
        if (shooter == null) return;

        if (bowTargetVfxPrefab != null)
            Instantiate(bowTargetVfxPrefab, target, Quaternion.identity);

        shooter.FireVolley(arrowCount, target, spreadRadius);
    }

    /// <summary>
    /// Vị trí mục tiêu: địch (không phải Base) gần cụm lính cung nhất.
    /// Không có địch -> điểm phía trước cụm lính cung.
    /// </summary>
    private Vector3 GetArrowTarget()
    {
        List<UnitType> bows = UnitRegistry.GetAlive(TroopType.Bow);
        Vector3 refPos = Vector3.zero;
        if (bows.Count > 0)
        {
            foreach (UnitType u in bows) refPos += u.transform.position;
            refPos /= bows.Count;
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Transform best = null;
        float bestSqr = float.MaxValue;
        foreach (Enemy e in enemies)
        {
            if (e == null || e.dead || e is EnemyBase) continue;
            float sqr = ((Vector2)e.transform.position - (Vector2)refPos).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = e.transform; }
        }

        if (best != null) return best.position;
        return refPos + new Vector3(5f, 0f, 0f); // fallback: phía trước (phe player hướng +X)
    }
}
