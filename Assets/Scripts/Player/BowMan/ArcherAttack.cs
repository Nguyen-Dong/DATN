using UnityEngine;

public class ArcherAttack : PlayerAttack
{
    private Archer archer;
    public Animator animator;

    [Header("Ranged Config")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Ballistic Config")]
    [Tooltip("Tốc độ ngang của mũi tên (giảm để tên không bay quá xa)")]
    [SerializeField] private float axesX = 7f;
    [Tooltip("Tốc độ dọc ban đầu (tạo vòng cung)")]
    [SerializeField] private float axesY = 5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Detection")]
    [SerializeField] private float detectRange = 10f;

    [HideInInspector] public bool attacking;

    // Lưu trạng thái detect để movement script dùng
    [HideInInspector] public bool isEnemyInRange = false;

    // Lưu trữ kẻ địch gần nhất hiện tại để bắn đón
    [HideInInspector] public Transform targetEnemy;

    // Đánh dấu archer đang di chuyển đến vị trí, chưa nên dừng để bắn
    [HideInInspector] public bool isMovingToPosition = false;

    private void Start()
    {
        archer = GetComponentInParent<Archer>();
        timer = coolDown;
    }

    private void Update()
    {
        if (!archer.CanMove()) return;

        isEnemyInRange = DetectEnemy();

        // Nếu đang di chuyển đến vị trí (retreat archer đang đi đến ArcherStandPoint)
        // thì KHÔNG dừng lại bắn, chỉ detect thôi
        if (isMovingToPosition)
        {
            attacking = false;
            animator.SetBool("Action", false);
            animator.SetBool("Ready", false);
            return;
        }

        if (isEnemyInRange)
        {
            // Dừng unit khi phát hiện địch trong tầm bắn
            Rigidbody2D rb = transform.parent.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Cập nhật Animator cho chế độ bắn cung
            animator.SetBool("Ready", true);
            animator.SetInteger("WeaponType", 3);

            if (CanAttack())
            {
                Attack();
            }
            else
            {
                // Đang cooldown - reset Action để animator có thể chuyển trạng thái
                // cho phép trigger SimpleBowShot hoạt động lại
                animator.SetBool("Action", false);
            }
        }
        else
        {
            // Không thấy địch -> reset trạng thái bắn
            attacking = false;
            animator.SetBool("Action", false);
            animator.SetBool("Ready", false);
        }
    }

    private void Attack()
    {
        attacked = true;
        attacking = true;

        // Set Action true cho frame này
        animator.SetBool("Action", true);

        // Animation bắn cung
        animator.SetTrigger("SimpleBowShot");

        // Gọi coroutine để sinh mũi tên (tránh phụ thuộc hoàn toàn vào Animation Event)
        StartCoroutine(ShootRoutine());
    }

    private System.Collections.IEnumerator ShootRoutine()
    {
        // Đợi một chút để đồng bộ với animation bắn cung (khoảng 0.3 - 0.5s tùy animation)
        yield return new WaitForSeconds(0.4f);

        if (arrowPrefab != null && shootPoint != null)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
            Arrow arrow = arrowObj.GetComponent<Arrow>();

            Vector2 velocity;

            if (targetEnemy != null)
            {
                // TÍNH TOÁN BẮN ĐÓN (PREDICTIVE BALLISTIC SHOOTING)
                Rigidbody2D enemyRb = targetEnemy.GetComponentInParent<Rigidbody2D>();
                Vector2 enemyVel = enemyRb != null ? enemyRb.linearVelocity : Vector2.zero;

                float distance = Vector2.Distance(shootPoint.position, targetEnemy.position);

                // Ước tính thời gian bay T của mũi tên dựa trên khoảng cách và tốc độ axesX
                float t = Mathf.Clamp(distance / axesX, 0.3f, 1.5f);

                // Dự đoán vị trí của enemy sau thời gian T
                Vector3 predictedPos = targetEnemy.position + (Vector3)enemyVel * t;

                // Lấy gravity từ Unity và gravityScale của Arrow
                float gravityScale = 1.5f; // Khớp với Arrow.cs
                Rigidbody2D arrowRb = arrowPrefab.GetComponent<Rigidbody2D>();
                if (arrowRb != null)
                {
                    gravityScale = arrowRb.gravityScale;
                }
                float g = -Physics2D.gravity.y * gravityScale;

                // Tính vận tốc phóng vx, vy để trúng predictedPos tại thời điểm t
                float vx = (predictedPos.x - shootPoint.position.x) / t;
                float vy = (predictedPos.y - shootPoint.position.y + 0.5f * g * t * t) / t;

                velocity = new Vector2(vx, vy);

                // Flip cung thủ theo hướng bắn
                float shootDir = vx > 0 ? 1f : -1f;
                Transform parentTransform = transform.parent;
                if (parentTransform != null)
                {
                    Vector3 currentScale = parentTransform.localScale;
                    currentScale.x = Mathf.Abs(currentScale.x) * shootDir;
                    parentTransform.localScale = currentScale;
                }
            }
            else
            {
                // Fallback nếu không có target
                float direction = transform.parent.localScale.x > 0 ? 1f : -1f;
                velocity = new Vector2(axesX * direction, axesY);
            }

            arrow.Initialize(damage, velocity);
        }

        // Reset attacking sau khi bắn xong để animator có thể chuẩn bị cho lần bắn tiếp
        attacking = false;
        animator.SetBool("Action", false);
    }

    /// <summary>
    /// Được gọi từ Animation Event khi animation bắn cung đến frame bắn.
    /// Giữ lại để tránh lỗi null reference nếu Animation Event vẫn đang được gọi.
    /// </summary>
    public void ShootEvent()
    {
        // Không làm gì ở đây nữa vì đã dùng Coroutine ShootRoutine
    }

    /// <summary>
    /// Phát hiện kẻ địch bằng OverlapCircle trong phạm vi hình tròn.
    /// Quét toàn bộ khu vực xung quanh thay vì chỉ bắn một tia thẳng.
    /// </summary>
    public bool DetectEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(shootPoint.position, detectRange, enemyLayer);
        if (hits == null || hits.Length == 0)
        {
            targetEnemy = null;
            return false;
        }

        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null && enemy.dead) continue;

            float dist = Vector2.Distance(shootPoint.position, hit.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestEnemy = hit.transform;
            }
        }

        targetEnemy = closestEnemy;
        return targetEnemy != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (shootPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(shootPoint.position, detectRange);
        }
    }
}
