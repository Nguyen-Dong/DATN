using UnityEngine;
using System;

public class GameResult : MonoBehaviour
{
    public static GameResult Instance { get; private set; }

    public event Action OnVictory;
    public event Action OnDefeat;

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
