using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Animator animator;
    private Vector2 moveInput;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector2 moveDirection = moveInput;
        transform.Translate(moveDirection * speed * Time.deltaTime);

        // Set the "Speed" parameter in the animator
        animator.SetFloat("Speed", moveDirection.magnitude);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "SwordLv1 (Enemies)")
        {
            Debug.Log("Player đánh Enemy!");
        }
    }
}
