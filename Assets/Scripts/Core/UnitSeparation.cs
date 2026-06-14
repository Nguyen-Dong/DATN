using UnityEngine;

/// <summary>
/// Component gắn trên root GameObject của mỗi unit (cùng level với Entity).
/// Phát hiện units cùng team ở gần và đẩy chúng ra xa nhau để tránh overlap.
/// Đẩy trên cả X và Y, nhưng giới hạn lực để không gây bay/drift.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class UnitSeparation : MonoBehaviour
{
    [Header("Separation Config")]
    [Tooltip("Bán kính phát hiện units gần")]
    [SerializeField] private float detectionRadius = 1.2f;

    [Tooltip("Khoảng cách tối thiểu muốn giữ giữa các unit")]
    [SerializeField] private float minDistance = 0.6f;

    [Tooltip("Lực đẩy separation trên trục X")]
    [SerializeField] private float separationForceX = 5f;

    [Tooltip("Layer của team mình (dùng để chỉ detect cùng team)")]
    [SerializeField] private LayerMask teamLayer;

    private Rigidbody2D rb;

    public void Initialize(LayerMask layer)
    {
        teamLayer = layer;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        Entity entity = GetComponent<Entity>();
        if (entity != null && entity.dead) return;

        ApplySeparation();
    }

    private void ApplySeparation()
    {
        Collider2D[] nearbyUnits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, teamLayer);

        float separationX = 0f;
        int neighborCount = 0;

        foreach (Collider2D col in nearbyUnits)
        {
            // Bỏ qua chính mình
            if (col.transform == transform) continue;

            // Bỏ qua unit đã chết
            Entity otherEntity = col.GetComponent<Entity>();
            if (otherEntity != null && otherEntity.dead) continue;

            float diffX = transform.position.x - col.transform.position.x;
            float absDistX = Mathf.Abs(diffX);

            if (absDistX < minDistance && absDistX > 0.01f)
            {
                // Lực đẩy tỷ lệ nghịch với khoảng cách
                float strength = (minDistance - absDistX) / minDistance;
                float direction = Mathf.Sign(diffX);
                separationX += direction * strength;
                neighborCount++;
            }
            else if (absDistX <= 0.01f)
            {
                // Chồng hoàn toàn X → đẩy ngẫu nhiên để tách ra
                separationX += (Random.value > 0.5f ? 1f : -1f) * 0.5f;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            separationX /= neighborCount;

            // Sử dụng dịch chuyển vị trí thay vì thay đổi vận tốc để tránh bị các script di chuyển ghi đè gây giật
            float maxSepVelocity = 1.5f; // Giới hạn tốc độ separation
            float separationVelX = Mathf.Clamp(separationX * separationForceX, -maxSepVelocity, maxSepVelocity);

            // Dịch chuyển trực tiếp rigidbody position theo DeltaTime
            Vector2 newPos = rb.position;
            newPos.x += separationVelX * Time.fixedDeltaTime;
            rb.position = newPos;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
}
