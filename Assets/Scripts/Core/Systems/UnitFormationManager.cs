using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [ĐÃ RÚT GỌN] Trước đây quản lý cả định vị đội hình (slot X/Y, giãn hàng...). Toàn bộ logic định vị/giãn
/// đã bị gỡ bỏ (gây xung đột/giật). Giờ chỉ còn là BỘ ĐẾM số quân mỗi phe (Player/Enemy) — phục vụ EnemyAI
/// (đếm quân sống để quyết định Tấn công/Phòng thủ/Rút lui). Cơ chế đội hình/tránh đè sẽ viết lại sau.
/// </summary>
public class UnitFormationManager : MonoBehaviour
{
    public static UnitFormationManager Instance { get; private set; }

    [Header("Đội hình phòng thủ Enemy (hàng dọc, mỗi cột 4 con)")]
    [Tooltip("Số enemy mỗi cột dọc")]
    [SerializeField] private int enemyUnitsPerRow = 4;
    [Tooltip("Khoảng cách giữa các cột (trục X - lùi dần về Base địch)")]
    [SerializeField] private float enemyRowSpacing = 1.2f;
    [Tooltip("Khoảng cách giữa các con trong cùng cột (trục Y) - nên > bán kính UnitSeparation để không xung đột")]
    [SerializeField] private float enemyColSpacing = 0.7f;

    private readonly List<Transform> playerUnits = new List<Transform>();
    private readonly List<Transform> enemyUnits = new List<Transform>();

    // Mốc Y mặt đất chung cho đội hình enemy (lấy từ con đăng ký đầu tiên)
    private float enemyGroundY;
    private bool enemyGroundYSet;

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
        if (unit != null && !playerUnits.Contains(unit))
            playerUnits.Add(unit);
    }

    public void UnregisterPlayerUnit(Transform unit)
    {
        playerUnits.Remove(unit);
    }

    public void RegisterEnemyUnit(Transform unit)
    {
        if (unit != null && !enemyUnits.Contains(unit))
            enemyUnits.Add(unit);
        if (!enemyGroundYSet && unit != null)
        {
            enemyGroundY = unit.position.y;
            enemyGroundYSet = true;
        }
    }

    /// <summary>
    /// Vị trí slot đội hình phòng thủ cho 1 enemy: xếp thành các CỘT DỌC (mỗi cột enemyUnitsPerRow con),
    /// các cột lùi dần về phía Base địch (+X). Trả về vị trí world (đã gồm mốc Y mặt đất).
    /// </summary>
    public Vector2 GetEnemyFormationSlot(Transform unit, float anchorX)
    {
        int index = enemyUnits.IndexOf(unit);
        if (index < 0) return new Vector2(anchorX, enemyGroundY);

        int row = index / enemyUnitsPerRow;     // cột thứ mấy (0 = tiền tuyến)
        int col = index % enemyUnitsPerRow;      // vị trí trong cột

        int total = enemyUnits.Count;
        int totalRows = (total - 1) / enemyUnitsPerRow + 1;
        int unitsInThisRow = (row < totalRows - 1) ? enemyUnitsPerRow : (total - row * enemyUnitsPerRow);

        float offsetX = row * enemyRowSpacing;                              // cột sau lùi về Base địch (+X)
        float offsetY = (col - (unitsInThisRow - 1) / 2f) * enemyColSpacing; // dàn dọc, căn giữa

        return new Vector2(anchorX + offsetX, enemyGroundY + offsetY);
    }

    public void UnregisterEnemyUnit(Transform unit)
    {
        enemyUnits.Remove(unit);
    }

    // ===== ĐẾM SỐ QUÂN SỐNG =====

    public int GetPlayerUnitCount() => playerUnits.Count;
    public int GetEnemyUnitCount() => enemyUnits.Count;

    // ===== DỌN NULL =====

    private void LateUpdate()
    {
        // Dọn các unit đã bị destroy mà chưa kịp Unregister
        playerUnits.RemoveAll(u => u == null);
        enemyUnits.RemoveAll(u => u == null);
    }
}
