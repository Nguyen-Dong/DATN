using UnityEngine;

public class SwordEnemyMovement : EnemyMovement
{
    protected SwordEnemy sword;
    protected int oldDirection;
    protected void Start()
    {
        Load();
        sword = GetComponentInParent<SwordEnemy>();
    }
    void Update()
    {
        if (!sword.CanMove()) return;
        if (sword.attack.DetectPlayer()) return;
        //if (ChangeDirectionCheck())
        //    directionMove *= -1;

        Move(directionMove);
    }
}
