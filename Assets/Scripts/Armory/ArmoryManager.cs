using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Nhánh nâng cấp: Vũ khí (+ATK) hoặc Giáp (+DEF).</summary>
public enum UpgradeKind { Weapon, Armor }

/// <summary>
/// UI Armory (panel trong Main). Cơ chế giống Shop:
/// - Mỗi loại lính có 2 nút (nâng Vũ khí / nâng Giáp) -> tổng các nút (3 nhân vật x 2).
/// - Click 1 nút -> hiện thông tin nhánh đó lên PANEL CHUNG.
/// - Panel chung có 1 nút Upgrade -> nâng cấp nhánh đang chọn (trừ điểm kỹ năng, tăng cấp ArmoryStore).
/// </summary>
public class ArmoryManager : MonoBehaviour
{
    /// <summary>Một nút nâng cấp gắn với (loại lính + nhánh).</summary>
    [Serializable]
    public class UpgradeButton
    {
        public Button button;
        public TroopUpgradeSO troop;
        public UpgradeKind kind;
    }

    [Header("Các nút nâng cấp (3 nhân vật x 2 nhánh)")]
    [SerializeField] private List<UpgradeButton> upgradeButtons = new List<UpgradeButton>();

    [Header("Panel chung")]
    [Tooltip("Panel thông tin - chỉ bật khi click 1 nút nâng cấp")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TMP_Text titleText;     // vd "Kiếm - Vũ khí"
    [SerializeField] private TMP_Text levelText;     // vd "Cấp 2/5"
    [SerializeField] private TMP_Text statText;      // vd "ATK 10 → 15"
    [SerializeField] private TMP_Text costText;      // giá điểm kỹ năng lên cấp kế / "MAX"
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeButtonText;

    [Header("Mở / đóng Armory")]
    [SerializeField] private GameObject armoryPanel;
    [SerializeField] private Button[] openButtons;
    [SerializeField] private Button closeButton;

    [Header("Preview (tùy chọn)")]
    [Tooltip("Hiển thị 3 nhân vật đổi visual theo cấp. Để trống nếu chưa làm preview.")]
    [SerializeField] private ArmoryPreview preview;

    private TroopUpgradeSO selectedTroop;
    private UpgradeKind selectedKind;

    private void Start()
    {
        foreach (UpgradeButton ub in upgradeButtons)
        {
            if (ub == null || ub.button == null || ub.troop == null) continue;
            TroopUpgradeSO t = ub.troop;
            UpgradeKind k = ub.kind;
            ub.button.onClick.AddListener(() => Select(t, k));
        }

        if (upgradeButton != null) upgradeButton.onClick.AddListener(UpgradeSelected);

        if (openButtons != null)
            foreach (Button b in openButtons)
                if (b != null) b.onClick.AddListener(OpenArmory);
        if (closeButton != null) closeButton.onClick.AddListener(CloseArmory);

        if (armoryPanel != null) armoryPanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    private void Update()
    {
        // Tướng chưa mở khóa -> tắt hẳn nút nâng cấp của nó
        foreach (UpgradeButton ub in upgradeButtons)
        {
            if (ub == null || ub.button == null || ub.troop == null) continue;
            ub.button.interactable = IsTroopUnlocked(ub.troop);
        }
    }

    private static bool IsTroopUnlocked(TroopUpgradeSO troop)
        => troop != null && (troop.unitDef == null || TroopUnlockStore.IsUnlocked(troop.unitDef));

    public void OpenArmory()
    {
        if (armoryPanel != null) armoryPanel.SetActive(true);
        selectedTroop = null;
        if (detailPanel != null) detailPanel.SetActive(false);
        if (preview != null) preview.RefreshAll();
    }

    public void CloseArmory()
    {
        if (detailPanel != null) detailPanel.SetActive(false);
        if (armoryPanel != null) armoryPanel.SetActive(false);
    }

    private void Select(TroopUpgradeSO troop, UpgradeKind kind)
    {
        selectedTroop = troop;
        selectedKind = kind;
        if (detailPanel != null) detailPanel.SetActive(true);
        Refresh();
    }

    private void Refresh()
    {
        if (selectedTroop == null) return;

        int lv = CurrentLevel();
        int max = MaxLevel();
        bool isMax = lv >= max;
        string statName = selectedKind == UpgradeKind.Weapon ? "ATK" : "DEF";

        if (titleText != null)
            titleText.text = $"{selectedTroop.troopType} - {(selectedKind == UpgradeKind.Weapon ? "Vũ khí" : "Giáp")}";

        if (levelText != null)
            levelText.text = $"Cấp {lv + 1}/{max + 1}";

        if (statText != null)
        {
            int cur = StatAt(lv);
            statText.text = isMax ? $"{statName} {cur} (MAX)" : $"{statName} {cur} → {StatAt(lv + 1)}";
        }

        if (costText != null)
            costText.text = isMax ? "MAX" : PriceAt(lv + 1).ToString();

        bool unlocked = IsTroopUnlocked(selectedTroop);
        if (upgradeButton != null) upgradeButton.interactable = !isMax && unlocked;
        if (upgradeButtonText != null) upgradeButtonText.text = !unlocked ? "Chưa mở khóa" : (isMax ? "MAX" : "Nâng cấp");
    }

    private void UpgradeSelected()
    {
        if (selectedTroop == null) return;

        int lv = CurrentLevel();
        if (lv >= MaxLevel()) return;

        int cost = PriceAt(lv + 1);
        if (CurrencyManager.Instance == null) return;

        if (CurrencyManager.Instance.TrySpendSkillPoints(cost))
        {
            SetLevel(lv + 1);
            Refresh();
            if (preview != null) preview.Refresh(selectedTroop.troopType);
        }
        else
        {
            Debug.Log("Armory: không đủ điểm kỹ năng.");
        }
    }

    // ===== Helpers theo nhánh đang chọn =====

    private int CurrentLevel() => selectedKind == UpgradeKind.Weapon
        ? ArmoryStore.GetWeaponLevel(selectedTroop.troopType)
        : ArmoryStore.GetArmorLevel(selectedTroop.troopType);

    private int MaxLevel() => selectedKind == UpgradeKind.Weapon
        ? selectedTroop.MaxWeaponLevel
        : selectedTroop.MaxArmorLevel;

    private int StatAt(int lv)
    {
        if (selectedKind == UpgradeKind.Weapon)
        {
            var w = selectedTroop.GetWeaponLevel(lv);
            return w != null ? w.atk : 0;
        }
        var a = selectedTroop.GetArmorLevel(lv);
        return a != null ? a.def : 0;
    }

    private int PriceAt(int lv)
    {
        if (selectedKind == UpgradeKind.Weapon)
        {
            var w = selectedTroop.GetWeaponLevel(lv);
            return w != null ? w.price : 0;
        }
        var a = selectedTroop.GetArmorLevel(lv);
        return a != null ? a.price : 0;
    }

    private void SetLevel(int lv)
    {
        if (selectedKind == UpgradeKind.Weapon)
            ArmoryStore.SetWeaponLevel(selectedTroop.troopType, lv);
        else
            ArmoryStore.SetArmorLevel(selectedTroop.troopType, lv);
    }
}
