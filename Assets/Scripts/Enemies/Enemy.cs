using UnityEngine;

public class Enemy : Entity
{
    [HideInInspector] public bool lieDown;

    public void StartLie(float time = 1)
    {
        lieDown = true;
        //rb.bodyType = RigidbodyType2D.Static;
        Invoke(nameof(EndLie), time);
    }
    private void EndLie()
    {
        lieDown = false;
        rb.AddForce(Vector3.up * 450);
        //rb.bodyType = RigidbodyType2D.Dynamic;
    }
    public bool CanMove()
    {
        if (dead || lieDown) return false;
        else return true;
    }

    public override void EntityDie()
    {
        if (!dead)
        {
            if (EnemyAI.Instance != null)
            {
                EnemyAI.Instance.RegisterEnemyDeath();
            }
        }
        base.EntityDie();
    }



    //[HideInInspector] public EnemyMovement movement;
    //// TODO: Add reference to an EnemyAttack script

    //private bool isAttacking = false; // Placeholder for attack logic

    //protected override void Start()
    //{
    //    base.Start();
    //    movement = GetComponent<EnemyMovement>();
    //}

    //private void Update()
    //{
    //    if (dead)
    //    {
    //        // If you have a death animation, the movement script might need to be handled differently
    //        return;
    //    }

    //    // A real implementation would get this from an EnemyAttack script
    //    if (isAttacking) 
    //    {
    //        movement.Move(0);
    //    }
    //    else
    //    {
    //        movement.Move(1); // Move forward (relative to orientation)
    //    }
    //}

    //// TODO: Implement logic to set isAttacking based on collision/range from an attack script.
    //// For example, an EnemyAttack script could raise an event or set a property on this class.
}