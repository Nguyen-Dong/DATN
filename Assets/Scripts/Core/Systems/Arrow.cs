using UnityEngine;

public class Arrow : MonoBehaviour
{
    private float damage;
    private float lifetime = 3f;
    private Rigidbody2D rb;
    private bool isFlying = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }
    public void Initialize(float arrowDamage, Vector2 vector2)
    {
        this.damage = arrowDamage;
        rb.linearVelocity = vector2;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if(isFlying && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    void OnTriggerEnter2D(Collider2D Collision)
    {
        if (Collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = Collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (Collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isFlying = false;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
            Destroy(gameObject);
        }
    }
}
