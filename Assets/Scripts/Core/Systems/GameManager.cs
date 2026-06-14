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
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] public Transform spawnPoint;
    [SerializeField] Button buySwordManBtn;

    [Header("Pause")]
    public Button pauseBtn;
    public GameObject pauseMenuPanel;
    public Button resumeBtn;
    public Button soundBtn;
    public Button quitBtn;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f; // Đảm bảo game không bị tạm dừng khi bắt đầu
    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    private void Start()
    {
        // Bỏ qua va chạm vật lý trực tiếp giữa các đơn vị cùng phe để di chuyển mượt mà không rung lắc cơ học
        Physics2D.IgnoreLayerCollision(6, 6, true); // Enemy vs Enemy
        Physics2D.IgnoreLayerCollision(7, 7, true); // Player vs Player

        UpdateGoldUI();
        buySwordManBtn.onClick.AddListener(BuySwordMan);

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
        if (TrySpendGold(50))
        {
            Instantiate(PlayerPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("Đã mua 1 Lính Cận Chiến!");
        }
    }
    private void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    private void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }
    private void QuitLevel()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("ReturnToMap", 1);
        SceneManager.LoadScene("Main");
    }
}
