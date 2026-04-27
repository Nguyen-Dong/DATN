using Unity.VisualScripting;
using UnityEngine;

public class SwordMovement : PlayerMovement
{
    protected Sword sword;
    protected int oldDirection;

    private Transform defendPoint;
    protected void Start()
    {
        Load();
        sword = GetComponentInParent<Sword>();

        GameObject dp = GameObject.Find("DefendPoint");
        if (dp != null)
        {
            defendPoint = dp.transform;
        }
    }
    void Update()
    {
        if (!sword.CanMove()) return;
        if (GameCommander.currentState == GameCommander.CommandState.Attack)
        {
            directionMove = 1f;
            if (sword.attack.DetectEnemy())
            {
                transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                if (anim != null)
                {
                    anim.SetInteger("State", 0);
                }
                return;
            }
        }
        else if (GameCommander.currentState == GameCommander.CommandState.Defend)
        {
            if (defendPoint != null && transform.parent.position.x > defendPoint.position.x)
            {
                directionMove = -1f;
            }
            else
            {
                directionMove = 0f;
                Flip(1);
                transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                if (anim != null)
                {
                    anim.SetInteger("State", 0);
                }
                return;
            }    
        }
        Flip((int)directionMove);

        Move(directionMove);
    }
    private void Flip(int dir)
    {
        if (dir == 0) return;
        Transform parentTranform = transform.parent;
        Vector3 currentScale = parentTranform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * dir;
        parentTranform.localScale = currentScale;
    }

    public void SetAnimator(Animator a)
    {
        anim = a;
    }
}
