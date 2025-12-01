using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 2f;
    private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    // Reference to the PlayerAttack script remains crucial
    public PlayerAttack playerAttack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // If the player is in attacking mode (colliding with an enemy)
        if (playerAttack != null && playerAttack.IsAttacking)
        {
            // Force stop movement and set animation to idle/attack-ready
            rb.linearVelocity = Vector2.zero;
            anim.SetInteger("State", 0); 
            return; // Exit early
        }

        // 1. Always move to the right
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

        // 2. Always set animation to Running state
        anim.SetInteger("State", 2);
    }
}
