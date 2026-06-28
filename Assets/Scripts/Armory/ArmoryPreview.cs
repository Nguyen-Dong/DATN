using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hiển thị preview các nhân vật trong Armory: áp visual theo cấp đã nâng.
/// Tướng CHƯA MỞ KHÓA -> tô màu đen (silhouette). Cache màu gốc để khôi phục khi mở khóa.
/// </summary>
public class ArmoryPreview : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        [Tooltip("GameObject nhân vật HeroEditor để hiển thị (có CharacterBodySculptor)")]
        public GameObject character;
        [Tooltip("Cấu hình nâng cấp của loại lính này")]
        public TroopUpgradeSO data;
    }

    [SerializeField] private List<Entry> previews = new List<Entry>();
    [Tooltip("Màu silhouette khi tướng chưa mở khóa")]
    [SerializeField] private Color lockedColor = Color.black;

    // Cache màu gốc từng SpriteRenderer để khôi phục khi tướng được mở khóa
    private readonly Dictionary<GameObject, SpriteRenderer[]> rendererCache = new Dictionary<GameObject, SpriteRenderer[]>();
    private readonly Dictionary<GameObject, Color[]> colorCache = new Dictionary<GameObject, Color[]>();

    private void Awake()
    {
        foreach (Entry e in previews)
        {
            if (e == null || e.character == null) continue;
            SpriteRenderer[] rs = e.character.GetComponentsInChildren<SpriteRenderer>(true);
            Color[] cs = new Color[rs.Length];
            for (int i = 0; i < rs.Length; i++) cs[i] = rs[i].color;
            rendererCache[e.character] = rs;
            colorCache[e.character] = cs;
        }
    }

    private void OnEnable() => RefreshAll();

    public void RefreshAll()
    {
        foreach (Entry e in previews) ApplyEntry(e);
    }

    public void Refresh(TroopType type)
    {
        foreach (Entry e in previews)
            if (e != null && e.data != null && e.data.troopType == type) ApplyEntry(e);
    }

    private void ApplyEntry(Entry e)
    {
        if (e == null || e.character == null || e.data == null) return;

        bool unlocked = e.data.unitDef == null || TroopUnlockStore.IsUnlocked(e.data.unitDef);

        if (unlocked)
        {
            RestoreColors(e.character);
            ArmoryVisual.Apply(e.character, e.data, applyStats: false);
        }
        else
        {
            // Chưa mở khóa -> silhouette đen
            SetColor(e.character, lockedColor);
        }
    }

    private void RestoreColors(GameObject character)
    {
        if (!rendererCache.TryGetValue(character, out SpriteRenderer[] rs)) return;
        Color[] cs = colorCache[character];
        for (int i = 0; i < rs.Length && i < cs.Length; i++)
            if (rs[i] != null) rs[i].color = cs[i];
    }

    private void SetColor(GameObject character, Color color)
    {
        if (!rendererCache.TryGetValue(character, out SpriteRenderer[] rs)) return;
        foreach (SpriteRenderer r in rs) if (r != null) r.color = color;
    }
}
