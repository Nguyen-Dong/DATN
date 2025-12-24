using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitLevel", menuName = "Game/Unit Level Data")]
public class UnitLevelSO : ScriptableObject
{
    [Header("Upgrade Info")]
    public string levelName;
    public int upgradeCost;

    [Header("Stats Modifiers")]
    public float maxHealth;
    public float damage;

    [Header("Visuals")]
    public GameObject visualPrefab;
}
