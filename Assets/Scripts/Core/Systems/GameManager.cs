using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Gold & Spawn")]
    [SerializeField] public int currentGold = 200;
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] public Transform spawnPoint;
    [SerializeField] Button buySwordManBtn;

    [Header("Mua lính (data-driven)")]
    [Tooltip("Định nghĩa lính kiếm. Nếu để trống sẽ dùng fallback PlayerPrefab bên dưới.")]
    [SerializeField] UnitDefinition swordManDef;

    [Header("Fallback (tương thích scene cũ)")]
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] int swordManGoldCost = 50;

    [Header("Test")]
    [Tooltip("Tốc độ game (để test cho nhanh). Đặt lại 1 khi release.")]
    [SerializeField] private float gameSpeed = 2f;

    [Header("Pause")]
    public Button pauseBtn;
    public GameObject pauseMenuPanel;
    public Button resumeBtn;
    public Button soundBtn;
    public Button quitBtn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.timeScale = gameSpeed; // Tốc độ game (test = 2x)
    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    private void Start()
    {
        // Bỏ qua va chạm vật lý trực tiếp giữa các đơn vị cùng phe để di chuyển mượt mà không rung lắc cơ học
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");
        if (enemyLayer >= 0) Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
        if (playerLayer >= 0) Physics2D.IgnoreLayerCollision(playerLayer, playerLayer, true);

        UpdateGoldUI();
        // Nút mua lính cũ (1 loại) - giờ thay bằng TroopBuyButtons (3 nút). Để trống cũng được.
        if (buySwordManBtn != null) buySwordManBtn.onClick.AddListener(BuySwordMan);

        pauseBtn.onClick.AddListener(PauseGame);
        resumeBtn.onClick.AddListener(ResumeGame);
        quitBtn.onClick.AddListener(QuitLevel);

        // Tạm thời chỉ in ra log cho nút Âm thanh
        soundBtn.onClick.AddListener(() => Debug.Log("Bật/Tắt Âm Thanh"));
    }
    private void UpdateGoldUI()
    {
        goldText.text = "Gold: " + currentGold.ToString();
    }
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
        Debug.Log($"Đã cộng {amount} vàng. Tổng: {currentGold}");
    }
    // Hàm kiểm tra và trừ tiền dùng chung cho việc mua lính
    public bool TrySpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldUI();
            return true;
        }
        Debug.Log("Không đủ vàng!");
        return false;
    }
    private void BuySwordMan()
    {
        // Ưu tiên dùng dữ liệu data-driven nếu đã gán
        if (swordManDef != null)
        {
            BuyUnit(swordManDef);
            return;
        }

        // Fallback: tương thích scene cũ (chưa tạo UnitDefinition)
        if (PlayerPrefab != null && TrySpendGold(swordManGoldCost))
        {
            Instantiate(PlayerPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("Đã mua 1 Lính Cận Chiến! (fallback)");
        }
    }

    /// <summary>
    /// Mua một loại lính theo <see cref="UnitDefinition"/>: phải ĐÃ MỞ KHÓA (ở Cửa hàng) và ĐỦ VÀNG.
    /// </summary>
    public bool BuyUnit(UnitDefinition def)
    {
        if (def == null || def.prefab == null)
        {
            Debug.LogWarning("GameManager.BuyUnit: UnitDefinition hoặc prefab null.");
            return false;
        }

        if (!TroopUnlockStore.IsUnlocked(def))
        {
            Debug.Log($"Lính '{def.displayName}' chưa mở khóa — hãy mua ở Cửa hàng.");
            return false;
        }

        if (!TrySpendGold(def.goldCost))
            return false;

        Instantiate(def.prefab, spawnPoint.position, Quaternion.identity);
        Debug.Log($"Đã mua lính: {def.displayName}");
        return true;
    }
    private void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    private void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = gameSpeed;
    }
    private void QuitLevel()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("ReturnToMap", 1);
        SceneManager.LoadScene("Main");
    }
}
