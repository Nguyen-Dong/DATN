using UnityEngine;

public class SwordMovement : PlayerMovement
{
    protected Sword sword;
    protected int oldDirection;
    protected void Start()
    {
        Load();
        sword = GetComponentInParent<Sword>();
    }
    void Update()
    {
        if (!sword.CanMove()) return;
        if (sword.attack.DetectEnemy())
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

    public void SetAnimator(Animator a)
    {
        anim = a;
    }
}
