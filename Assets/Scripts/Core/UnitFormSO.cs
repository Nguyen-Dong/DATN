using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitForm", menuName = "Game/Unit Form Data")]
public class UnitFormSO : ScriptableObject
{
    [Header("Identity")]
    public string formName;
    public GameObject visualPrefab;

    [Header("Evolution Cost")]
    public int evolveCost;

    [Header("Base Stats")]
    public float baseDamage;
    public float baseHealth;
    public float baseSpeed;

    [Header("Upgrade Config")]
    public float damagePerLevel;
    public float healthPerLevel;
    public float speedPerLevel;
}
