using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUIManager : MonoBehaviour
{
    public static UpgradeUIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject upgradePanel;

    [Header("Button")]
    [SerializeField] private Button btnDamage;
    [SerializeField] private Button btnHealth;
    [SerializeField] private Button btnSpeed;
    [SerializeField] private Button btnEvolve;

    [Header("Indicators")]
    [SerializeField] private TextMeshProUGUI txtDamageLvl;
    [SerializeField] private TextMeshProUGUI txtHealthLvl;
    [SerializeField] private TextMeshProUGUI txtSpeedLvl;
    [SerializeField] private TextMeshProUGUI txtEvolveCost;

    private StatUpgrader _currentUnit;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        HidePanel();
    }

    public void OpenPanel(StatUpgrader unit)
    {
        _currentUnit = unit;
        upgradePanel.SetActive(true);
        UpdateUI();
    }

    public void HidePanel()
    {
        upgradePanel.SetActive(false);
        _currentUnit = null;
    }

    private void UpdateUI()
    {
        if (_currentUnit == null) return;

        txtDamageLvl.text = _currentUnit.DamageLevel.ToString();
        txtHealthLvl.text = _currentUnit.HealthLevel.ToString();
        txtSpeedLvl.text = _currentUnit.SpeedLevel.ToString();
        
        if (_currentUnit.IsMaxForm())
        {
            btnEvolve.interactable = false;
            txtEvolveCost.text = "MAX LEVEL";
        } else
        {
            btnEvolve.interactable = true;
            txtEvolveCost.text = _currentUnit.GetNextEvolveCost().ToString();
        }
    }

    public void OnClickDamage()
    {
        if (_currentUnit != null && _currentUnit.TryUpgradeDamage()) UpdateUI();
    }
    public void OnClickHealth()
    {
        if (_currentUnit != null && _currentUnit.TryUpgradeHealth()) UpdateUI();
    }
    public void OnClickSpeed()
    {
        if (_currentUnit != null && _currentUnit.TryUpgradeSpeed()) UpdateUI();
    }

    public void OnClickEvolve()
    {
        if (_currentUnit != null && _currentUnit.TryEvole()) UpdateUI();
    }
}
