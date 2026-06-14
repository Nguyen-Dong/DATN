using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton quản lý formation slots cho tất cả units (cả Player lẫn Enemy).
/// Mỗi unit đăng ký vào đây khi spawn, hủy đăng ký khi chết.
/// Manager tính toán slot position cho từng unit dựa trên thứ tự đăng ký.
/// </summary>
public class UnitFormationManager : MonoBehaviour
{
    public static UnitFormationManager Instance { get; private set; }

    [Header("Formation Config")]
    [Tooltip("Số unit tối đa trên mỗi hàng")]
    [SerializeField] private int unitsPerRow = 4;

    [Tooltip("Khoảng cách giữa các hàng (trục X - trước/sau)")]
    [SerializeField] private float rowSpacing = 1.5f;

    [Tooltip("Khoảng cách giữa các unit trong cùng hàng (trục Y - trên/dưới)")]
    [SerializeField] private float colSpacing = 0.8f;

    // Danh sách units theo phe
    private List<Transform> playerUnits = new List<Transform>();
    private List<Transform> enemyUnits = new List<Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ===== ĐĂNG KÝ / HỦY ĐĂNG KÝ =====

    public void RegisterPlayerUnit(Transform unit)
    {
        if (!playerUnits.Contains(unit))
            playerUnits.Add(unit);
    }

    public void UnregisterPlayerUnit(Transform unit)
    {
        playerUnits.Remove(unit);
    }

    public void RegisterEnemyUnit(Transform unit)
    {
        if (!enemyUnits.Contains(unit))
            enemyUnits.Add(unit);
    }

    public void UnregisterEnemyUnit(Transform unit)
    {
        enemyUnits.Remove(unit);
    }

    // ===== TÍNH TOÁN FORMATION =====

    /// <summary>
    /// Lấy formation offset cho một player unit.
    /// Trả về Vector2 offset so với vị trí frontline.
    /// </summary>
    public Vector2 GetPlayerFormationOffset(Transform unit)
    {
        return GetFormationOffset(unit, playerUnits, 1); // Player đi sang phải (+X)
    }

    /// <summary>
    /// Lấy formation offset cho một enemy unit.
    /// Trả về Vector2 offset so với vị trí frontline.
    /// </summary>
    public Vector2 GetEnemyFormationOffset(Transform unit)
    {
        return GetFormationOffset(unit, enemyUnits, -1); // Enemy đi sang trái (-X)
    }

    /// <summary>
    /// Lấy row index (hàng) của một player unit. Row 0 = tiền tuyến.
    /// </summary>
    public int GetPlayerRow(Transform unit)
    {
        return GetRow(unit, playerUnits);
    }

    /// <summary>
    /// Lấy row index (hàng) của một enemy unit. Row 0 = tiền tuyến.
    /// </summary>
    public int GetEnemyRow(Transform unit)
    {
        return GetRow(unit, enemyUnits);
    }

    /// <summary>
    /// Lấy tổng số player units đã đăng ký.
    /// </summary>
    public int GetPlayerUnitCount()
    {
        return playerUnits.Count;
    }

    /// <summary>
    /// Lấy tổng số enemy units đã đăng ký.
    /// </summary>
    public int GetEnemyUnitCount()
    {
        return enemyUnits.Count;
    }

    // ===== PRIVATE HELPERS =====

    private int GetRow(Transform unit, List<Transform> unitList)
    {
        int index = unitList.IndexOf(unit);
        if (index < 0) return 0;
        return index / unitsPerRow;
    }

    private Vector2 GetFormationOffset(Transform unit, List<Transform> unitList, int facingDirection)
    {
        int index = unitList.IndexOf(unit);
        if (index < 0) return Vector2.zero;

        int row = index / unitsPerRow;
        int col = index % unitsPerRow;

        // Tính số unit thực tế trong hàng này
        int totalUnits = unitList.Count;
        int totalRows = (totalUnits - 1) / unitsPerRow + 1;
        int unitsInThisRow;
        if (row < totalRows - 1)
        {
            unitsInThisRow = unitsPerRow;
        }
        else
        {
            unitsInThisRow = totalUnits - row * unitsPerRow;
        }

        // offsetX: hàng sau lùi về phía sau (ngược hướng di chuyển)
        float offsetX = -row * rowSpacing * facingDirection;

        // offsetY: dàn ngang trong hàng, căn giữa
        float offsetY = (col - (unitsInThisRow - 1) / 2f) * colSpacing;

        return new Vector2(offsetX, offsetY);
    }

    // ===== CLEANUP =====

    /// <summary>
    /// Dọn dẹp các unit đã bị destroy khỏi danh sách.
    /// Gọi định kỳ hoặc khi cần thiết.
    /// </summary>
    public void CleanupNullUnits()
    {
        playerUnits.RemoveAll(u => u == null);
        enemyUnits.RemoveAll(u => u == null);
    }

    private void LateUpdate()
    {
        // Tự động dọn dẹp null references mỗi frame
        // (phòng trường hợp unit bị destroy mà chưa kịp Unregister)
        CleanupNullUnits();
    }
}
