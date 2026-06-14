using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class GameCommander : MonoBehaviour
{
    public static event Action<CommandState> OnCommandStateChanged;
    public enum CommandState { Defend, Attack, Retreat }
    // Mặc định khi mua lính sẽ ở chế độ Phòng thủ
    public static CommandState currentState = CommandState.Defend;

    [Header("State Buttons")]
    [SerializeField] public Button btnAttack;
    [SerializeField] public Button btnDefend;
    [SerializeField] public Button btnRetreat;

    [Header("Button Colors")]
    [Tooltip("Màu nút khi đang được chọn (xám đi)")]
    [SerializeField] private Color activeColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [Tooltip("Màu nút bình thường")]
    [SerializeField] private Color normalColor = Color.white;

    [Header("Retreat Archer Config")]
    [Tooltip("Số lính cung sẽ được spawn khi rút lui lần đầu")]
    [SerializeField] private int retreatArcherCount = 3;
    [Tooltip("Prefab lính cung")]
    [SerializeField] private GameObject archerPrefab;
    [Tooltip("Vị trí lính cung sẽ đứng khi rút lui (đích đến)")]
    [SerializeField] private Transform archerStandPoint;
    [Tooltip("Scale cho lính cung retreat")]
    [SerializeField] private float archerScale = 0.55f;

    // Lưu trữ danh sách lính cung rút lui đã spawn (không spawn lại)
    private List<GameObject> retreatArchers = new List<GameObject>();
    private bool hasSpawnedRetreatArchers = false;

    private void Start()
    {
        currentState = CommandState.Defend;
        btnAttack.onClick.AddListener(SetAttackCommand);
        btnDefend.onClick.AddListener(SetDefendCommand);
        btnRetreat.onClick.AddListener(SetRetreatCommand);

        // Cập nhật trạng thái nút ban đầu - mặc định là Defend
        UpdateButtonVisuals();
    }

    public void SetAttackCommand()
    {
        if (currentState == CommandState.Attack) return;
        currentState = CommandState.Attack;
        Debug.Log("Toàn quân: Tấn công!");
        UpdateButtonVisuals();
        OnCommandStateChanged?.Invoke(currentState);
    }

    public void SetDefendCommand()
    {
        if (currentState == CommandState.Defend) return;
        currentState = CommandState.Defend;
        Debug.Log("Toàn quân: Phòng thủ!");
        UpdateButtonVisuals();
        OnCommandStateChanged?.Invoke(currentState);
    }

    public void SetRetreatCommand()
    {
        if (currentState == CommandState.Retreat) return;
        currentState = CommandState.Retreat;
        Debug.Log("Toàn quân: Rút lui!");
        UpdateButtonVisuals();
        OnCommandStateChanged?.Invoke(currentState);

        // Lần đầu bấm rút lui: spawn 3 lính cung
        // Các lần sau: chỉ gọi lại lính cung đã có (ArcherMovement tự xử lý)
        if (!hasSpawnedRetreatArchers)
        {
            SpawnRetreatArchers();
        }
        // Nếu đã spawn rồi thì không cần làm gì,
        // ArcherMovement sẽ tự chuyển hành vi khi detect currentState == Retreat
    }

    /// <summary>
    /// Cập nhật màu nút: nút đang active sẽ xám đi, các nút khác trở về màu bình thường.
    /// </summary>
    private void UpdateButtonVisuals()
    {
        SetButtonColor(btnAttack, currentState == CommandState.Attack ? activeColor : normalColor);
        SetButtonColor(btnDefend, currentState == CommandState.Defend ? activeColor : normalColor);
        SetButtonColor(btnRetreat, currentState == CommandState.Retreat ? activeColor : normalColor);
    }

    private void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;
        // Thay đổi màu Image của button
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = color;
        }
    }

    /// <summary>
    /// Spawn đội lính cung hỗ trợ lần đầu tiên.
    /// Lính cung được spawn tại spawnPoint (cùng chỗ mua lính),
    /// sau đó sẽ tự di chuyển ra archerStandPoint.
    /// </summary>
    private void SpawnRetreatArchers()
    {
        // Lấy spawnPoint từ GameManager (cùng chỗ mua lính)
        Transform unitSpawnPoint = null;
        if (GameManager.Instance != null)
        {
            unitSpawnPoint = GameManager.Instance.spawnPoint;
        }

        if (archerPrefab == null)
        {
            Debug.LogWarning("GameCommander: Chưa gán archerPrefab!");
            return;
        }

        if (unitSpawnPoint == null)
        {
            Debug.LogWarning("GameCommander: Không tìm thấy spawnPoint từ GameManager!");
            return;
        }

        // Xóa reference cũ nếu có (phòng trường hợp)
        retreatArchers.Clear();

        for (int i = 0; i < retreatArcherCount; i++)
        {
            // Spawn tại vị trí spawn lính (phía sau), lệch Y một chút
            Vector3 spawnPos = unitSpawnPoint.position + new Vector3(0, i * 0.3f, 0);
            GameObject archer = Instantiate(archerPrefab, spawnPos, Quaternion.identity);

            // Đặt scale 0.55f
            archer.transform.localScale = new Vector3(archerScale, archerScale, archerScale);

            // Đánh dấu là lính cung rút lui
            archer.tag = "RetreatArcher";

            // Lưu reference
            retreatArchers.Add(archer);

            Debug.Log($"Spawn lính cung hỗ trợ rút lui #{i + 1} tại spawnPoint");
        }

        hasSpawnedRetreatArchers = true;
    }
}
