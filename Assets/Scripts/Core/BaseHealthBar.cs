using UnityEngine;
using UnityEngine.UI;

public class BaseHealthBar : MonoBehaviour
{
    private Entity baseEntity;
    public Slider healthSlider; // Kéo thả UI Slider vào đây

    private float maxHealth;

    private void Start()
    {
        baseEntity = GetComponent<Entity>();
    }

    private void Update()
    {
        if (baseEntity != null && healthSlider != null)
        {
            healthSlider.maxValue = baseEntity.GetMaxHealth();
            healthSlider.value = baseEntity.GetCurrentHealth();
        }
    }
}
