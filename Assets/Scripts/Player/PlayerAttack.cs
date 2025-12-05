using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float damage = 3;
    [SerializeField] protected float coolDown = 1;
    protected float timer = 1;
    [HideInInspector] public bool attacked; // used for the CanAttack method
    protected bool CanAttack()
    {
        if (attacked)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                return false;
            }
            else
            {
                attacked = false;
                timer = coolDown;
                return true;
            }
        }
        else
        {
            return true;
        }
    }
    //[Header("Attack Settings")]
    //public float damage = 10f;
    //public float attackRadius = 1f;
    //public float attackCooldown = 0.5f;

    //[Header("References")]
    //public Transform attackPoint;
    //public LayerMask enemyLayers;
    //public Animator animator;

    //public bool IsAttacking { get; private set; } // Public property to signal state
    //private float cooldownTimer = 0f;

    //void Update()
    //{
    //    // Cooldown timer always counts down
    //    if (cooldownTimer > 0)
    //    {
    //        cooldownTimer -= Time.deltaTime;
    //    }

    //    // If currently set to attack automatically and cooldown is ready
    //    if (IsAttacking && cooldownTimer <= 0)
    //    {
    //        Attack();
    //        cooldownTimer = attackCooldown; // Reset cooldown
    //    }
    //}

    //void Attack()
    //{
    //    if (animator != null)
    //    {
    //        animator.SetTrigger("Slash");                                                                           
    //        animator.SetInteger("WeaponType", 0);
    //    }

    //    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);

    //    foreach (Collider2D enemy in hitEnemies)
    //    {
    //        Debug.Log("We hit " + enemy.name);
    //        Entity enemyEntity = enemy.GetComponent<Entity>();
    //        if (enemyEntity != null)
    //        {
    //            enemyEntity.TakeDamage(damage); 
    //        }
    //    }
    //}

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if ((enemyLayers.value & (1 << collision.gameObject.layer)) > 0)
    //    {
    //        Debug.Log("Started touching an enemy.");
    //        IsAttacking = true; // Start auto-attacking
    //    }
    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if ((enemyLayers.value & (1 << collision.gameObject.layer)) > 0)
    //    {
    //        Debug.Log("Stopped touching an enemy.");
    //        IsAttacking = false; // Stop auto-attacking
    //    }
    //}

    //void OnDrawGizmosSelected()
    //{
    //    if (attackPoint == null) return;
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    //}
}