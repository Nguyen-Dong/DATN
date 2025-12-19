using UnityEngine;

public class SwordEnemyMovement : EnemyMovement
{
    protected SwordEnemy sword_E;
    protected int oldDirection;
    protected void Start()
    {
        Load();
        sword_E = GetComponentInParent<SwordEnemy>();
    }
    void Update()
    {
        if (!sword_E.CanMove()) return;
        if (sword_E.attack.DetectPlayer()) return;
        //if (ChangeDirectionCheck())
        //    directionMove *= -1;

        Move(directionMove);
    }
}
