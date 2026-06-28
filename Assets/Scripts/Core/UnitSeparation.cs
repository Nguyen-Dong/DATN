using UnityEngine;

/// <summary>
/// Tách nhẹ các unit CÙNG PHE để không đè 100% lên nhau.
///
/// Thiết kế để KHÔNG giật (khác bản cũ):
/// - Chạy ở LateUpdate (sau khi FSM/di chuyển đã chạy) -> là lần ghi vị trí CUỐI CÙNG trong frame.
/// - Đẩy TẤT ĐỊNH: khi 2 unit trùng vị trí, tách theo InstanceID (con nhỏ lên, con lớn xuống) -> không random.
/// - Giới hạn dịch tối đa mỗi frame -> không văng.
/// - KHÔNG kéo về vị trí gốc -> không có "kéo-co", tự đạt cân bằng rồi đứng yên.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class UnitSeparation : MonoBehaviour
{
    [Tooltip("Bán kính coi là 'đè nhau' (đẩy cho tới khi cách nhau khoảng này)")]
    [SerializeField] private float radius = 0.45f;

    [Tooltip("Dịch tối đa mỗi frame (world units) - càng nhỏ càng mượt, càng chậm tách")]
    [SerializeField] private float maxPushPerFrame = 0.05f;

    private LayerMask teamMask;
    private bool teamMaskSet;
    private Entity entity;

    public void Initialize(LayerMask mask)
    {
        teamMask = mask;
        teamMaskSet = true;
    }

    private void Awake()
    {
        entity = GetComponent<Entity>();
        if (!teamMaskSet)
        {
            teamMask = 1 << gameObject.layer; // mặc định: cùng layer = cùng phe
            teamMaskSet = true;
        }
    }

    private void LateUpdate()
    {
        if (entity != null && entity.dead) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, teamMask);
        Vector2 push = Vector2.zero;
        int count = 0;

        foreach (Collider2D c in hits)
        {
            if (c.transform == transform) continue;
            Entity other = c.GetComponent<Entity>();
            if (other != null && other.dead) continue;

            Vector2 diff = (Vector2)transform.position - (Vector2)c.transform.position;
            float dist = diff.magnitude;
            if (dist >= radius) continue;

            Vector2 dir;
            if (dist > 0.001f)
                dir = diff / dist;
            else
                // Trùng vị trí gần như hệt -> tách dọc TẤT ĐỊNH theo InstanceID
                dir = (transform.GetInstanceID() < c.transform.GetInstanceID()) ? Vector2.down : Vector2.up;

            float strength = (radius - dist) / radius; // càng gần càng đẩy mạnh
            push += dir * strength;
            count++;
        }

        if (count == 0) return;

        push /= count;
        if (push.sqrMagnitude > 1f) push.Normalize();

        transform.position = (Vector2)transform.position + push * maxPushPerFrame;
    }
}
