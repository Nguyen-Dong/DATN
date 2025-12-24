using UnityEngine;

public class SwordEnemyAttack : EnemyAttack
{
    private SwordEnemy sword_E;
    public Animator animator;

    [Header("Attack Zone")]
    [SerializeField] protected GameObject[] attackPoints;

    [SerializeField] protected float radius;
    [HideInInspector] public bool attacking;  // attacking == true => Can't move;
    private void Start()
    {
        sword_E = GetComponentInParent<SwordEnemy>();
        timer = coolDown;
    }
    private void Update()
    {
        if (!sword_E.CanMove()) return;
        bool playerDetected = DetectPlayer();
        animator.SetBool("Ready", playerDetected);
        if (playerDetected)
        {
            if (CanAttack())
            {
                Attack();
            }
        }
        
    }
    private void Attack()
    {
        if (DetectPlayer())
        {
            attacked = true;
            attacking = true;
            animator.SetTrigger("Slash");
            animator.SetInteger("WeaponType", 0);
        }
    }

    public void AttackEven()
    {
        foreach (GameObject point in attackPoints)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(point.transform.position, radius);
            foreach (Collider2D col in cols)
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    Player player = col.gameObject.GetComponent<Player>();

                    //Do some thing
                    player.TakeDamage(damage);

                }
            }
        }
    }
    public bool DetectPlayer()
    {
        foreach (GameObject point in attackPoints)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(point.transform.position, radius);
            foreach (Collider2D col in cols)
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (GameObject point in attackPoints)
        {
            Gizmos.DrawWireSphere(point.transform.position, radius);
        }

        //Gizmos.DrawWireSphere(airAttackPoint.transform.position, airRadius);
    }
}
