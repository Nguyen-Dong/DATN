using UnityEngine;

public class ArcherMovement : PlayerMovement
{
    protected Archer archer;
    protected int oldDirection;
    protected void Start()
    {
        Load();
        archer = GetComponentInParent<Archer>();
    }
    void Update()
    {
        if (!archer.CanMove()) return;
        if (archer.attack.DetectEnemy())
        {
            transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            if (anim != null)
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
