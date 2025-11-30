using UnityEngine;

public class EnemieMovement : MonoBehaviour
{
    public float speed = 2f;
    private Rigidbody2D rb;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Only move if speed is greater than 0
        if (speed > 0)
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
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

    public void SetHead()
    {
        // Thêm logic bạn muốn thực thi ở đây.
       // Ví dụ, để kiểm tra, bạn có thể thêm một dòng debug:
        Debug.Log("SetHead event was called!");
    }
}
