using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

/// <summary>
/// Quản lý TIỀN META (ngoài trận), tồn tại xuyên scene và lưu bằng PlayerPrefs:
/// - Kim cương (Diamonds): mua chiêu ở Cửa hàng.
/// - Điểm kỹ năng (SkillPoints): nâng cấp lính ở Armory.
///
/// LƯU Ý: Vàng (Gold) trong trận đánh do <see cref="GameManager"/> quản lý RIÊNG — không liên quan tới class này.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private const string KEY_DIAMONDS = "Meta_Diamonds";
    private const string KEY_SKILLPOINTS = "Meta_SkillPoints";

    [Header("Số tiền mặc định (lần đầu chơi, chưa có save)")]
    [Tooltip("Số kim cương khởi tạo khi chưa có dữ liệu lưu")]
    [SerializeField] private int defaultDiamonds = 0;
    [Tooltip("Số điểm kỹ năng khởi tạo khi chưa có dữ liệu lưu")]
    [SerializeField] private int defaultSkillPoints = 0;

    [Header("UI (tùy chọn)")]
    [Tooltip("Text hiển thị số kim cương")]
    [FormerlySerializedAs("goldText")] // giữ tham chiếu cũ đã kéo thả trong scene
    [SerializeField] private TextMeshProUGUI diamondText;
    [Tooltip("Text hiển thị số điểm kỹ năng")]
    [SerializeField] private TextMeshProUGUI skillPointText;

    public int Diamonds { get; private set; }
    public int SkillPoints { get; private set; }

    /// <summary>Phát khi kim cương hoặc điểm kỹ năng thay đổi (cho UI khác lắng nghe).</summary>
    public event System.Action OnChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        // Chưa có key (lần đầu chơi) -> lấy số mặc định; có rồi -> lấy giá trị đã lưu
        Diamonds = PlayerPrefs.GetInt(KEY_DIAMONDS, defaultDiamonds);
        SkillPoints = PlayerPrefs.GetInt(KEY_SKILLPOINTS, defaultSkillPoints);
        RefreshUI();
    }

    private void Save()
    {
        PlayerPrefs.SetInt(KEY_DIAMONDS, Diamonds);
        PlayerPrefs.SetInt(KEY_SKILLPOINTS, SkillPoints);
        PlayerPrefs.Save();
    }

    // ===== KIM CƯƠNG =====

    public void AddDiamonds(int amount)
    {
        if (amount == 0) return;
        Diamonds = Mathf.Max(0, Diamonds + amount);
        Save();
        RefreshUI();
        OnChanged?.Invoke();
    }

    public bool TrySpendDiamonds(int amount)
    {
        if (amount < 0 || Diamonds < amount) return false;
        Diamonds -= amount;
        Save();
        RefreshUI();
        OnChanged?.Invoke();
        return true;
    }

    // ===== ĐIỂM KỸ NĂNG =====

    public void AddSkillPoints(int amount)
    {
        if (amount == 0) return;
        SkillPoints = Mathf.Max(0, SkillPoints + amount);
        Save();
        RefreshUI();
        OnChanged?.Invoke();
    }

    public bool TrySpendSkillPoints(int amount)
    {
        if (amount < 0 || SkillPoints < amount) return false;
        SkillPoints -= amount;
        Save();
        RefreshUI();
        OnChanged?.Invoke();
        return true;
    }

    // ===== UI =====

    private void RefreshUI()
    {
        if (diamondText != null) diamondText.text = Diamonds.ToString();
        if (skillPointText != null) skillPointText.text = SkillPoints.ToString();
    }

#if UNITY_EDITOR
    [ContextMenu("DEBUG: +100 Kim cương, +5 Điểm kỹ năng")]
    private void DebugGrant() { AddDiamonds(100); AddSkillPoints(5); }

    [ContextMenu("DEBUG: Reset tiền meta về 0")]
    private void DebugReset()
    {
        Diamonds = 0; SkillPoints = 0;
        Save(); RefreshUI(); OnChanged?.Invoke();
    }
#endif
}
