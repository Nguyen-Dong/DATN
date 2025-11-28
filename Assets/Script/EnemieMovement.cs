using UnityEngine;

public class EnemieMovement : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "SwordLv1")
        {
            Debug.Log("Enemy đánh Player!");
        }
    }
}
