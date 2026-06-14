using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public bool createdDust;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;
    [SerializeField] public bool dead;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f; // Tắt trọng lực để tránh sụt lún cơ học gây rung giật khi Lerp đội hình Y

            // Tự động gán UnitSeparation cho các thực thể di động thuộc phe Player (layer 7) hoặc Enemy (layer 6) nếu chưa có
            if (GetComponent<UnitSeparation>() == null)
            {
                if (gameObject.layer == 6 || gameObject.layer == 7)
                {
                    UnitSeparation sep = gameObject.AddComponent<UnitSeparation>();
                    // Thiết lập LayerMask cho cùng phe
                    LayerMask mask = 1 << gameObject.layer;
                    sep.Initialize(mask);
                }
            }
        }
    }
    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage.");
        if (currentHealth <= 0)
        {
            EntityDie();
        }
    }
    public void TakeHealth(float health)
    {
        currentHealth += health;
    }
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
    }
    public void HealFull()
    {
        currentHealth = maxHealth;
    }
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;

    public virtual void EntityDie()
    {
        dead = true;
        if (gameObject.GetComponent<Collider2D>() != null)
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

}