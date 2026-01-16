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

    [Header("Damage UI")]
    [SerializeField] private TextMeshProUGUI txtDamageLvl;
    [SerializeField] private TextMeshProUGUI txtDamageCost;

    [Header("Health UI")]
    [SerializeField] private TextMeshProUGUI txtHealthLvl;
    [SerializeField] private TextMeshProUGUI txtHealthCost;

    [Header("Speed UI")]
    [SerializeField] private TextMeshProUGUI txtSpeedLvl;
    [SerializeField] private TextMeshProUGUI txtSpeedCost;

    [Header("Evolve UI")]
    [SerializeField] private TextMeshProUGUI txtEvolveLvl;
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

        //Damage
        txtDamageLvl.text = "Lv. " + _currentUnit.DamageLevel.ToString();
        if(_currentUnit.DamageLevel >= 5)
        {
            txtDamageLvl.text = "MAX";
            btnDamage.interactable = false;
        }
        else
        {
            txtDamageCost.text = _currentUnit.GetStatUpgradeCost().ToString();
            btnDamage.interactable = true;
        }
        //Health
        txtHealthLvl.text = "Lv. " + _currentUnit.HealthLevel.ToString();
        if (_currentUnit.HealthLevel >= 5)
        {
            txtHealthLvl.text = "MAX";
            btnHealth.interactable = false;
        }
        else
        {
            txtHealthCost.text = _currentUnit.GetStatUpgradeCost().ToString();
            btnHealth.interactable = true;
        }
        //Speed
        txtSpeedLvl.text = "Lv. " + _currentUnit.SpeedLevel.ToString();
        if (_currentUnit.SpeedLevel >= 5)
        {
            txtSpeedLvl.text = "MAX";
            btnSpeed.interactable = false;
        }
        else
        {
            txtSpeedCost.text = _currentUnit.GetStatUpgradeCost().ToString();
            btnSpeed.interactable = true;
        }
        //Evolve
        txtEvolveLvl.text = "Lv. " + (_currentUnit.CurrentFormIndex + 1).ToString();
        if (_currentUnit.IsMaxForm())
        {
            txtEvolveCost.text = "MAX LV";
            btnEvolve.interactable = false;
        } else
        {
            txtEvolveCost.text = _currentUnit.GetNextEvolveCost().ToString();
            btnEvolve.interactable = true;            
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
