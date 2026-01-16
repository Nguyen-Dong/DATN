using UnityEngine;
using System.Collections.Generic;
using Assets.HeroEditor.Common.CharacterScripts;

public class StatUpgrader : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private List<UnitFormSO> forms;
    [SerializeField] private int statUpgradeCost = 50;
    [SerializeField] private int maxStatLevel = 5;

    public int CurrentFormIndex { get; private set; } = 0;
    public int DamageLevel { get; private set; } = 1;
    public int HealthLevel { get; private set; } = 1;
    public int SpeedLevel { get; private set; } = 1;

    private Entity _entity;
    private EntityAttack _entityAttack;
    private EntityMovement _entityMovement;
    private LayerManager _visualContainer;

    private void Start()
    {
        _entity = GetComponent<Entity>();
        _entityAttack = GetComponentInChildren<EntityAttack>();
        _entityMovement = GetComponentInChildren<EntityMovement>();
        _visualContainer = GetComponent<LayerManager>();
        
        if(forms.Count > 0)
        {             
            ApplyForm(forms[0]);
        }
    }

    public int GetStatUpgradeCost()
    {
        return statUpgradeCost;
    }

    public bool TryEvole()
    {
        if (CurrentFormIndex >= forms.Count - 1) return false;

        UnitFormSO nextForm = forms[CurrentFormIndex + 1];
        if(CurrencyManager.Instance.TrySpendGold(nextForm.evolveCost))
        {
            CurrentFormIndex++;

            DamageLevel = 1;
            HealthLevel = 1;
            SpeedLevel = 1;
            ApplyForm(forms[CurrentFormIndex]);
            return true;
        }
        return false;
    }

    public bool TryUpgradeDamage()
    {
        if (DamageLevel >= maxStatLevel) return false;
        if (CurrencyManager.Instance.TrySpendGold(statUpgradeCost))
        {
            DamageLevel++;
            RecalculateStats();
            return true;
        }
        return false;
    }

    public bool TryUpgradeHealth()
    {
        if (HealthLevel >= maxStatLevel) return false;
        if (CurrencyManager.Instance.TrySpendGold(statUpgradeCost))
        {
            HealthLevel++;
            RecalculateStats();
            return true;
        }
        return false;
    }

    public bool TryUpgradeSpeed()
    {
        if (SpeedLevel >= maxStatLevel) return false;
        if (CurrencyManager.Instance.TrySpendGold(statUpgradeCost))
        {
            SpeedLevel++;
            RecalculateStats();
            return true;
        }
        return false;
    }

    private void ApplyForm(UnitFormSO form)
    {
        if(form.visualPrefab != null && _visualContainer != null)
        {
            for(int i = 0; i < _visualContainer.Sprites.Count;i++)
            {
                LayerManager layerManager = form.visualPrefab.GetComponent<LayerManager>();
                if (_visualContainer.Sprites[i] != null && layerManager.Sprites[i] != null)
                {
                    _visualContainer.Sprites[i].sprite = layerManager.Sprites[i].sprite;
                    _visualContainer.Sprites[i].color = layerManager.Sprites[i].color;
                }
                
            }
         
        }

        RecalculateStats();
        if(_entity != null)
        {
            _entity.HealFull();
        }
    }

    private void RecalculateStats()
    {
        UnitFormSO currentData = forms[CurrentFormIndex];
        if(_entityAttack != null)
        {
            _entityAttack.damage = currentData.baseDamage + (currentData.damagePerLevel * (DamageLevel - 1));
        }
        if(_entity != null)
        {
            float maxHealth = currentData.baseHealth + (currentData.healthPerLevel * (HealthLevel - 1));
            _entity.SetMaxHealth(maxHealth);
        }
        if(_entityMovement != null)
        {
            _entityMovement.speed = currentData.baseSpeed + (currentData.speedPerLevel * (SpeedLevel - 1));
        }
    }

    public int GetNextEvolveCost()
    {
        if (forms == null || CurrentFormIndex >= forms.Count - 1) return 0;
        return forms[CurrentFormIndex + 1].evolveCost;
    }

    public bool IsMaxForm()
    {
        return CurrentFormIndex >= forms.Count - 1;
    }
}
