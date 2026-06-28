using UnityEngine;
using System;

public class GameResult : MonoBehaviour
{
    public static GameResult Instance { get; private set; }

    public event Action OnVictory;
    public event Action OnDefeat;

    [Header("Phần thưởng khi thắng")]
    [Tooltip("Kim cương nhận được mỗi lần thắng")]
    [SerializeField] private int victoryDiamondReward = 50;
    [Tooltip("Điểm kỹ năng nhận được mỗi lần thắng")]
    [SerializeField] private int victorySkillPointReward = 2;
    [Tooltip("Các tướng được MỞ KHÓA khi thắng màn này (dùng được để mua trong trận sau)")]
    [SerializeField] private UnitDefinition[] unlockOnVictory;

    /// <summary>Phát khi 1 tướng vừa được mở khóa do thắng (cho UI thông báo). Tham số: tướng vừa mở.</summary>
    public event Action<UnitDefinition> OnTroopUnlocked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerVictory()
    {
        Debug.Log("VICTORY!");
        Time.timeScale = 0f; // Pause game

        // Thưởng tiền meta (kim cương + điểm kỹ năng) - persist qua CurrencyManager
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddDiamonds(victoryDiamondReward);
            CurrencyManager.Instance.AddSkillPoints(victorySkillPointReward);
        }

        // Mở khóa tướng mới khi thắng màn này
        if (unlockOnVictory != null)
        {
            foreach (UnitDefinition def in unlockOnVictory)
            {
                if (def == null || TroopUnlockStore.IsUnlocked(def)) continue;
                TroopUnlockStore.Unlock(def);
                Debug.Log($"GameResult: Mở khóa tướng mới: {def.displayName}");
                OnTroopUnlocked?.Invoke(def);
            }
        }

        OnVictory?.Invoke();

        // Update unlock level
        int currentLevel = PlayerPrefs.GetInt("CurrentPlayingLevel", 0);
        int highestUnlockLevel = PlayerPrefs.GetInt("HighestUnlockLevel", 0);
        
        if (currentLevel >= highestUnlockLevel)
        {
            PlayerPrefs.SetInt("HighestUnlockLevel", currentLevel + 1);
            PlayerPrefs.Save();
        }
    }

    public void TriggerDefeat()
    {
        Debug.Log("DEFEAT!");
        Time.timeScale = 0f; // Pause game
        OnDefeat?.Invoke();
    }
}
