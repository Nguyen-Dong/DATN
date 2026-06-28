using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý Cửa hàng: danh sách button item (icon + giá + SO), một panel chi tiết dùng chung
/// (hiển thị tên + mô tả + nút Mua) chỉ hiện khi click vào một item.
/// Mỗi item có thể MUA NHIỀU LẦN -> tích số lượng (count) theo từng loại lính, lưu persist.
/// Số lượng này sẽ được dùng trong gameplay sau (vd: số lần triệu hồi/dùng chiêu).
/// </summary>
public class ShopManager : MonoBehaviour
{
    /// <summary>Một ô item trên Cửa hàng (gán trong Inspector).</summary>
    [Serializable]
    public class ShopItemButton
    {
        [Tooltip("Nút bấm của item")]
        public Button button;
        [Tooltip("Ảnh icon của item")]
        public Image icon;
        [Tooltip("Text hiển thị giá tiền")]
        public TMP_Text priceText;
        [Tooltip("Text hiển thị số lượng đang có (tùy chọn)")]
        public TMP_Text countText;
        [Tooltip("Dữ liệu item tương ứng (ShopItemSO)")]
        public ShopItemSO data;
    }

    [Header("Danh sách item")]
    [SerializeField] private List<ShopItemButton> items = new List<ShopItemButton>();

    [Header("Panel chi tiết (chung)")]
    [Tooltip("Panel hiển thị thông tin item được chọn - chỉ bật khi click vào item")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [Tooltip("Text hiển thị số lượng đang có của item được chọn (tùy chọn)")]
    [SerializeField] private TMP_Text ownedCountText;
    [SerializeField] private Button buyButton;
    [Tooltip("Text trên nút Mua (tùy chọn)")]
    [SerializeField] private TMP_Text buyButtonText;

    [Header("Mở / đóng Cửa hàng")]
    [Tooltip("Panel tổng của Cửa hàng (toàn bộ cửa sổ shop)")]
    [SerializeField] private GameObject shopPanel;
    [Tooltip("Các nút bấm để MỞ Cửa hàng (có thể gán nhiều nút)")]
    [SerializeField] private Button[] openButtons;
    [Tooltip("Nút ĐÓNG Cửa hàng (tùy chọn)")]
    [SerializeField] private Button closeButton;

    private ShopItemSO selected;

    private const string COUNT_PREFIX = "ShopCount_";

    private void Start()
    {
        // Thiết lập từng button item: đổ icon + giá + số lượng từ SO, gắn sự kiện click
        foreach (ShopItemButton item in items)
        {
            if (item == null || item.data == null) continue;

            if (item.icon != null && item.data.icon != null)
                item.icon.sprite = item.data.icon;

            if (item.priceText != null)
                item.priceText.text = item.data.price.ToString();

            if (item.button != null)
            {
                ShopItemSO captured = item.data; // tránh bug closure trong vòng lặp
                item.button.onClick.AddListener(() => Select(captured));
            }
        }

        if (buyButton != null)
            buyButton.onClick.AddListener(BuySelected);

        // Gắn các nút mở / đóng Cửa hàng
        if (openButtons != null)
            foreach (Button b in openButtons)
                if (b != null) b.onClick.AddListener(OpenShop);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);

        RefreshItemCounts();

        // Cửa hàng + panel chi tiết ẩn lúc đầu
        if (shopPanel != null) shopPanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    /// <summary>Mở Cửa hàng (gọi từ các nút openButtons hoặc code khác).</summary>
    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        // Chưa chọn item -> ẩn panel chi tiết
        selected = null;
        if (detailPanel != null) detailPanel.SetActive(false);
        RefreshItemCounts();
    }

    /// <summary>Đóng Cửa hàng.</summary>
    public void CloseShop()
    {
        if (detailPanel != null) detailPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    /// <summary>Click vào một item -> chọn nó và mở panel chi tiết.</summary>
    private void Select(ShopItemSO data)
    {
        selected = data;
        if (detailPanel != null)
            detailPanel.SetActive(true);
        RefreshDetail();
    }

    /// <summary>Cập nhật nội dung panel theo item đang chọn.</summary>
    private void RefreshDetail()
    {
        if (selected == null) return;

        if (nameText != null) nameText.text = selected.displayName;
        if (descriptionText != null) descriptionText.text = selected.description;
        if (ownedCountText != null) ownedCountText.text = GetCount(selected.troopType).ToString();
        if (buyButtonText != null) buyButtonText.text = $"Mua ({selected.price})";

        // Mua nhiều lần -> nút Mua luôn bật (việc đủ/thiếu tiền do TrySpendDiamonds xử lý)
        if (buyButton != null) buyButton.interactable = true;
    }

    /// <summary>Click nút Mua -> mua item đang chọn (cộng số lượng).</summary>
    private void BuySelected()
    {
        if (selected == null) return;

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("ShopManager: thiếu CurrencyManager.");
            return;
        }

        if (CurrencyManager.Instance.TrySpendDiamonds(selected.price))
        {
            int newCount = AddCount(selected.troopType, 1);
            Debug.Log($"ShopManager: đã mua {selected.displayName} ({selected.troopType}). Số lượng: {newCount}");
            RefreshDetail();
            RefreshItemCounts();
        }
        else
        {
            Debug.Log("ShopManager: không đủ kim cương!");
        }
    }

    /// <summary>Cập nhật text số lượng trên các button item.</summary>
    private void RefreshItemCounts()
    {
        foreach (ShopItemButton item in items)
        {
            if (item == null || item.data == null || item.countText == null) continue;
            item.countText.text = GetCount(item.data.troopType).ToString();
        }
    }

    // ===== Số lượng đã mua theo loại lính (PlayerPrefs) =====

    /// <summary>Số lượng hiện có của một loại chiêu (dùng cho gameplay sau này).</summary>
    public static int GetCount(TroopType type) => PlayerPrefs.GetInt(COUNT_PREFIX + type, 0);

    /// <summary>Cộng số lượng, trả về số mới.</summary>
    private static int AddCount(TroopType type, int amount)
    {
        int value = Mathf.Max(0, GetCount(type) + amount);
        PlayerPrefs.SetInt(COUNT_PREFIX + type, value);
        PlayerPrefs.Save();
        return value;
    }

    /// <summary>Trừ số lượng khi dùng trong gameplay (trả về true nếu còn để dùng).</summary>
    public static bool TryConsume(TroopType type, int amount = 1)
    {
        int current = GetCount(type);
        if (current < amount) return false;
        PlayerPrefs.SetInt(COUNT_PREFIX + type, current - amount);
        PlayerPrefs.Save();
        return true;
    }
}
