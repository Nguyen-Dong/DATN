using UnityEngine;

public class EnemieMovement : MonoBehaviour
{
    public float speed = 2f;
    private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    void Update()
    {
        // Only move if speed is greater than 0
        if (speed > 0)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            anim.SetInteger("State", 2);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Use CompareTag for better performance and flexibility.
        // Make sure your Player GameObject has the "Player" tag.
        if (collision.gameObject.name == "SwordLv1")
        {
            Debug.Log("Enemy attacks Player!");
            speed = 0f; // Stop movement
            anim.SetTrigger("Attack"); // Trigger the Attack animation
        }
    }
}
