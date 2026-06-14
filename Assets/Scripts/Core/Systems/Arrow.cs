using UnityEngine;

public class Arrow : MonoBehaviour
{
    private float damage;
    private float lifetime = 5f;
    private Rigidbody2D rb;
    private bool isFlying = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Đảm bảo gravity hoạt động để tên bay vòng cung rồi rơi xuống
        if (rb != null)
        {
            rb.gravityScale = 1.5f; // Tăng gravity để tên rơi nhanh hơn, không bay quá xa
        }
    }

    public void Initialize(float arrowDamage, Vector2 velocity)
    {
        this.damage = arrowDamage;
        rb.linearVelocity = velocity;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isFlying && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            // Xoay mũi tên theo hướng bay
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isFlying) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            StopAndDestroy();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StopAndStick();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFlying) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            StopAndDestroy();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StopAndStick();
        }
    }

    /// <summary>
    /// Mũi tên cắm xuống đất - dừng lại và tự hủy sau 2 giây.
    /// </summary>
    private void StopAndStick()
    {
        isFlying = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;
        // Tự hủy sau 2 giây (tên cắm trên đất rồi biến mất)
        Destroy(gameObject, 2f);
    }

    /// <summary>
    /// Mũi tên trúng enemy - hủy ngay.
    /// </summary>
    private void StopAndDestroy()
    {
        isFlying = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Destroy(gameObject);
    }
}
