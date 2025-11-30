using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
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
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            anim.SetInteger("State", 2);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.name == "SwordLv1 (Enimies)")
        {
            Debug.Log("Player attacks Enemy!");
            speed = 0f; // Stop movement
            anim.SetTrigger("Attack"); 
        }
    }

}
