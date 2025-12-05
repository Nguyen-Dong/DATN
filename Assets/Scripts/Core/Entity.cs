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
    }
    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            EntityDie();
        }
    }
    public void TakeHealth(float health)
    {
        currentHealth += health;
    }
    public void EntityDie()
    {
        dead = true;
        gameObject.GetComponent<Collider2D>().enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
    }

}