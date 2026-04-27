using UnityEngine;
using UnityEngine.UI;
using System;

public class GameCommander : MonoBehaviour
{
    public static event Action<CommandState> OnCommandStateChanged;
    public enum CommandState { Attack, Defend }
    public static CommandState currentState = CommandState.Attack;

    [Header("State Buttons")]
    [SerializeField] public Button btnAttack;
    [SerializeField] public Button btnDefend;

    private void Start()
    {
        btnAttack.onClick.AddListener(SetAttackCommand);
        btnDefend.onClick.AddListener(SetDefendCommand);
    }
    public void SetAttackCommand()
    {
        currentState = CommandState.Attack;
        Debug.Log("Toàn quân: Tấn công");
        OnCommandStateChanged?.Invoke(currentState);
    }
    public void SetDefendCommand()
    {
        currentState = CommandState.Defend;
        Debug.Log("Toàn quân: Rút lui");
        OnCommandStateChanged?.Invoke(currentState);
    }    
}
