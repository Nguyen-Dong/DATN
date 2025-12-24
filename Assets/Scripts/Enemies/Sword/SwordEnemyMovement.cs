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
        if (sword_E.attack.DetectPlayer())
        {
            transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            if(anim != null)
            {
                anim.SetInteger("State", 0);
            }
            return;
        }
        //if (ChangeDirectionCheck())
        //    directionMove *= -1;

        Move(directionMove);
    }
}
