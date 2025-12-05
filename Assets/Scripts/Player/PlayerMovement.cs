using UnityEngine;

public class PlayerMovement : EntityMovement
{
    protected Enemy m_enemy;
    public int directionMove = 1;
    [SerializeField] private Animator anim;
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
        if (direction > 0)
            transform.parent.localScale = new Vector3(1, 1, 1);
        //if (direction < 0)
        //    transform.parent.localScale = new Vector3(-1, 1, 1);
        Debug.Log("Move Player");
        Vector2 velocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        transform.parent.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(direction * speed, velocity.y);
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
    //    Collider2D[] colliders = Physics2D.OverlapBoxAll(pointGroundCheck.transform.position, sizeBoxCheckGround, 0);
    //    foreach (Collider2D collider in colliders)
    //    {
    //        if (LayerMask.LayerToName(collider.gameObject.layer) == "Ground")
    //            return true;
    //    }
    //    return false;
    //}
    //public override bool WallCheck()
    //{
    //    Collider2D[] colliders = Physics2D.OverlapBoxAll(pointWallCheck.transform.position, sizeBoxCheckWall, 0);
    //    foreach (Collider2D collider in colliders)
    //    {
    //        if (LayerMask.LayerToName(collider.gameObject.layer) == "Ground")
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireCube(pointGroundCheck.transform.position, sizeBoxCheckGround);
    //    Gizmos.DrawWireCube(pointWallCheck.transform.position, sizeBoxCheckWall);
    //}




    //private Rigidbody2D rb;


    //private void Awake()
    //{
    //    rb = GetComponentInParent<Rigidbody2D>();
    //}

    //public override void Move(float direction)
    //{
    //    if (direction == 0)
    //    {
    //        // Stop movement
    //        rb.linearVelocity = Vector2.zero;
    //        if (anim != null) anim.SetInteger("State", 0);
    //        return;
    //    }

    //    // Move in the given direction
    //    rb.linearVelocity = new Vector2(speed * direction, rb.linearVelocity.y);

    //    // Set animation to Running state
    //    if (anim != null) anim.SetInteger("State", 2);
    //}

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