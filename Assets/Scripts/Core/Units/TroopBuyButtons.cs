using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 3 nút mua lính trong trận: mỗi nút gắn 1 UnitDefinition, bấm thì mua loại đó (qua GameManager.BuyUnit).
/// Nút tự mờ khi: chưa mở khóa loại đó (nếu bật requireUnlock) hoặc không đủ vàng.
/// </summary>
public class TroopBuyButtons : MonoBehaviour
{
    [Serializable]
    public class BuySlot
    {
        public Button button;
        public UnitDefinition def;
        [Tooltip("Text hiển thị giá vàng (tùy chọn)")]
        public TMP_Text costText;
    }

    [SerializeField] private List<BuySlot> slots = new List<BuySlot>();

    [Tooltip("Chỉ cho mua loại lính đã MỞ KHÓA (TroopUnlockStore). BuyUnit cũng tự kiểm tra điều này.")]
    [SerializeField] private bool requireUnlock = true;

    private void Start()
    {
        foreach (BuySlot s in slots)
        {
            if (s == null || s.button == null || s.def == null) continue;
            UnitDefinition d = s.def;
            s.button.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.BuyUnit(d);
            });
            if (s.costText != null) s.costText.text = d.goldCost.ToString();
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        foreach (BuySlot s in slots)
        {
            if (s == null || s.button == null || s.def == null) continue;
            bool unlocked = !requireUnlock || TroopUnlockStore.IsUnlocked(s.def);
            bool affordable = GameManager.Instance.currentGold >= s.def.goldCost;
            s.button.interactable = unlocked && affordable;
        }
    }
}
