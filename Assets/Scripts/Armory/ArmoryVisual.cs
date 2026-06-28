using System.Collections.Generic;
using UnityEngine;
using Assets.HeroEditor.Common.CharacterScripts;

/// <summary>
/// Helper dùng chung để áp cấp Armory (đọc từ ArmoryStore) lên một nhân vật HeroEditor:
/// - Visual: copy nhóm VŨ KHÍ / GIÁP. Phân nhóm bằng CharacterBodySculptor
///   (vũ khí = MeleeWeapon/Shield/Bow/Firearm; giáp = các part còn lại; vũ khí là CON của tay nên giáp loại trừ chúng).
///   Ghép sprite giữa prefab nguồn và lính theo ĐƯỜNG DẪN part (không theo index) -> không bị gán chéo dù khác thứ tự/số lượng.
/// - (tùy chọn) Chỉ số: gán ATK (EntityAttack.damage) + DEF (Entity.SetDefense).
/// </summary>
public static class ArmoryVisual
{
    public static void Apply(GameObject target, TroopUpgradeSO data, bool applyStats)
    {
        if (target == null || data == null) return;

        TroopUpgradeSO.WeaponLevel w = data.GetWeaponLevel(ArmoryStore.GetWeaponLevel(data.troopType));
        TroopUpgradeSO.ArmorLevel a = data.GetArmorLevel(ArmoryStore.GetArmorLevel(data.troopType));

        if (applyStats)
        {
            EntityAttack atk = target.GetComponentInChildren<EntityAttack>();
            if (w != null && atk != null) atk.damage = w.atk;

            Entity ent = target.GetComponent<Entity>();
            if (a != null && ent != null) ent.SetDefense(a.def);
        }

        CharacterBodySculptor body = target.GetComponentInChildren<CharacterBodySculptor>();
        if (body == null)
        {
            Debug.LogWarning($"[ArmoryVisual] '{target.name}' không có CharacterBodySculptor -> không đổi được visual.");
            return;
        }

        if (w != null && w.visualPrefab != null) ApplyGroup(body, w.visualPrefab, weapon: true);
        if (a != null && a.visualPrefab != null) ApplyGroup(body, a.visualPrefab, weapon: false);
    }

    /// <summary>Copy sprite của một nhóm (vũ khí/giáp) từ prefab nguồn sang lính, GHÉP THEO ĐƯỜNG DẪN part.</summary>
    private static void ApplyGroup(CharacterBodySculptor dstBody, GameObject sourcePrefab, bool weapon)
    {
        CharacterBodySculptor srcBody = sourcePrefab.GetComponentInChildren<CharacterBodySculptor>();
        if (srcBody == null)
        {
            Debug.LogWarning($"[ArmoryVisual] prefab '{sourcePrefab.name}' không có CharacterBodySculptor.");
            return;
        }

        // Lập bản đồ sprite nguồn theo đường dẫn (chỉ trong nhóm cần copy)
        List<Transform> srcWeaponRoots = WeaponRoots(srcBody);
        var srcMap = new Dictionary<string, SpriteRenderer>();
        foreach (SpriteRenderer sr in srcBody.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (IsUnderAny(sr.transform, srcWeaponRoots) != weapon) continue;
            srcMap[GetPath(sr.transform, srcBody.transform)] = sr;
        }

        // Áp lên các part cùng đường dẫn ở lính
        List<Transform> dstWeaponRoots = WeaponRoots(dstBody);
        int copied = 0;
        foreach (SpriteRenderer dr in dstBody.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (IsUnderAny(dr.transform, dstWeaponRoots) != weapon) continue;
            if (srcMap.TryGetValue(GetPath(dr.transform, dstBody.transform), out SpriteRenderer sr))
            {
                dr.sprite = sr.sprite;
                dr.color = sr.color;
                copied++;
            }
        }

        Debug.Log($"[ArmoryVisual] copy {(weapon ? "VŨ KHÍ" : "GIÁP")} từ '{sourcePrefab.name}': {copied} part (ghép theo path).");
    }

    /// <summary>Đường dẫn của part tính từ gốc nhân vật (vd "Torso/Head") - dùng để ghép đúng part giữa 2 nhân vật.</summary>
    private static string GetPath(Transform t, Transform root)
    {
        string path = t.name;
        Transform cur = t.parent;
        while (cur != null && cur != root)
        {
            path = cur.name + "/" + path;
            cur = cur.parent;
        }
        return path;
    }

    private static bool IsUnderAny(Transform t, List<Transform> roots)
    {
        foreach (Transform root in roots)
        {
            if (root == null) continue;
            if (t.IsChildOf(root)) return true; // IsChildOf bao gồm cả chính nó
        }
        return false;
    }

    private static List<Transform> WeaponRoots(CharacterBodySculptor c)
    {
        var list = new List<Transform>();
        if (c.MeleeWeapon != null) list.AddRange(c.MeleeWeapon);
        list.Add(c.Shield);
        if (c.Bow != null) list.AddRange(c.Bow);
        list.Add(c.Firearm);
        return list;
    }
}
