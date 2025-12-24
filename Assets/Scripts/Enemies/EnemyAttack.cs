using UnityEngine;

public class EnemyAttack : EntityAttack
{
    protected void Start()
    {
        timer = coolDown;
    }
}
