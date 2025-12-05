using UnityEngine;

public class EnemyMovement : EntityMovement
{
    private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }

    private void Start()
    {
        // Enemies face left by default
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    public override void Move(float direction)
    {
        if (direction == 0)
        {
            // Stop moving, could be idle or attacking
            if (anim != null) anim.SetInteger("State", 0);
            transform.Translate(Vector2.zero);
            return;
        }

        // Move in the given direction (relative to the enemy's orientation)
        transform.Translate(Vector2.right * speed * direction * Time.deltaTime);
        if (anim != null) anim.SetInteger("State", 2);
    }
    public override void Jump()
    {
        
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