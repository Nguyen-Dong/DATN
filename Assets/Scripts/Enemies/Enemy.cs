using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Enemy : Entity
{
    [HideInInspector] public EnemyMovement movement;
    // TODO: Add reference to an EnemyAttack script

    private bool isAttacking = false; // Placeholder for attack logic

    protected override void Start()
    {
        base.Start();
        movement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        if (dead)
        {
            // If you have a death animation, the movement script might need to be handled differently
            return;
        }

        // A real implementation would get this from an EnemyAttack script
        if (isAttacking) 
        {
            movement.Move(0);
        }
        else
        {
            movement.Move(1); // Move forward (relative to orientation)
        }
    }

    // TODO: Implement logic to set isAttacking based on collision/range from an attack script.
    // For example, an EnemyAttack script could raise an event or set a property on this class.
}