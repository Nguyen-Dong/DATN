using UnityEngine;

public class EnemyMovement : EntityMovement
{
    protected Enemy m_enemy;
    public int directionMove = -1;
    [SerializeField] protected Animator anim;
    [SerializeField] protected GameObject pointChangeDirCheck;

    protected void Reset()
    {
        Load();
    }
    protected void Load()
    {
        m_enemy = transform.parent.GetComponent<Enemy>();
        //pointGroundCheck = transform.Find("GroundCheck").gameObject;
        //pointWallCheck = transform.Find("WallCheck").gameObject;
    }


    public override void Move(float direction)
    {
        //if (direction > 0)
        //    transform.parent.localScale = new Vector3(1, 1, 1);
        if (direction < 0)
            transform.parent.localScale = new Vector3(-0.55f, 0.55f, 0.55f);
        // Ép Y = 0 để lính luôn đi NGANG, không bị trôi chéo do vận tốc Y dư
        transform.parent.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(direction * speed, 0f);
        anim.SetInteger("State", 2);
    }
    public override void Jump()
    {

    }
    protected bool ChangeDirectionCheck()
    {
        Debug.DrawRay(pointChangeDirCheck.transform.position, Vector3.down);
        RaycastHit2D colliderCheckGround = Physics2D.Raycast(pointChangeDirCheck.transform.position, Vector3.down, 1, LayerMask.GetMask("Ground"));
        Debug.DrawRay(pointChangeDirCheck.transform.position, Vector3.left * transform.parent.localScale.x);
        RaycastHit2D collidersCheckWall = Physics2D.Raycast(pointChangeDirCheck.transform.position, Vector3.left * transform.parent.localScale.x, 1, LayerMask.GetMask("Ground"));

        if (colliderCheckGround.collider == null)
        {
            return true;
        }
        if (collidersCheckWall.collider != null)
        {
            return true;
        }
        return false;
    }

    //public override bool GroundCheck()
    //{
    //    // TODO: Implement proper ground check
    //    return true;
    //}

    //public override bool WallCheck()
    //{
    //    // TODO: Implement proper wall check
    //    return false;
    //}
}