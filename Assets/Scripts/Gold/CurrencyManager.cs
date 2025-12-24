using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }
    [SerializeField] private int currentGold = 0;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI goldText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        UpdateUI(); // Cập nhật ngay khi vào game
    }

    public int CurrentGold => currentGold;

    public void AddGold(int amount)
    {
        Debug.Log($"Gold added: {amount}. Total: {currentGold}");
        currentGold += amount;
    }
    public bool TrySpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            Debug.Log($"Gold spent: {amount}. Remaining: {currentGold}");
            return true;
        }
        Debug.Log($"Not enough gold to spend: {amount}. Current: {currentGold}");
        return false;
    }

    // Hàm phụ trách việc đổi số trên màn hình
    private void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + currentGold.ToString();
        }
    }
}
