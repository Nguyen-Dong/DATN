using UnityEngine;

public class ArcherAttack : PlayerAttack
{
    private Archer archer;
    public Animator animator;

    [Header("Ranged Config")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;

    /// <summary>Prefab mũi tên cung thủ dùng - để hệ chiêu mưa tên dùng đúng mũi tên (cùng kích thước).</summary>
    public GameObject ArrowPrefab => arrowPrefab;

    [Header("Ballistic Config")]
    [Tooltip("Tốc độ ngang của mũi tên (giảm để tên không bay quá xa)")]
    [SerializeField] private float axesX = 7f;
    [Tooltip("Tốc độ dọc ban đầu (tạo vòng cung)")]
    [SerializeField] private float axesY = 5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Detection")]
    [SerializeField] private float detectRange = 10f;
    [Tooltip("Khi vừa phát hiện địch (đang đi tới đứng lại): đứng idle bao lâu trước khi bắt đầu bắn (giây)")]
    [SerializeField] private float idleBeforeAttack = 0.35f;

    // Theo dõi việc vừa vào tầm để chèn 1 nhịp idle trước khi bắn
    private bool wasInRange = false;
    private float settleTimer = 0f;

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
        // Lệch nhẹ tầm phát hiện để mỗi cung thủ dừng bắn ở cự ly hơi khác nhau -> không trụ chồng 1 điểm
        detectRange *= Random.Range(0.9f, 1.1f);
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
            wasInRange = false;
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

            // Vừa vào tầm -> bắt đầu 1 nhịp ĐỨNG IDLE trước khi bắn
            if (!wasInRange) settleTimer = idleBeforeAttack;
            wasInRange = true;

            if (settleTimer > 0f)
            {
                // Đang trong nhịp idle: chưa giương cung, chưa bắn
                settleTimer -= Time.deltaTime;
                attacking = false;
                animator.SetBool("Action", false);
                animator.SetBool("Ready", false);
            }
            else
            {
                // Hết nhịp idle -> giương cung và bắn
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
        }
        else
        {
            // Không thấy địch -> reset trạng thái bắn
            attacking = false;
            animator.SetBool("Action", false);
            animator.SetBool("Ready", false);
            wasInRange = false;
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
    /// Bắn 1 LOẠT count mũi tên CÙNG LÚC, rải quanh targetPos (dùng cho chiêu). Bắn theo ballistic như thường.
    /// </summary>
    public void FireVolley(int count, Vector3 targetPos, float spread)
    {
        if (arrowPrefab == null || shootPoint == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 aim = targetPos + (Vector3)(Random.insideUnitCircle * spread);
            Vector2 velocity = BallisticVelocityTo(aim);

            GameObject arrowObj = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
            Arrow arrow = arrowObj.GetComponent<Arrow>();
            if (arrow != null) arrow.Initialize(damage, velocity);
        }

        // Quay mặt về phía mục tiêu
        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            float dir = targetPos.x >= parentTransform.position.x ? 1f : -1f;
            Vector3 s = parentTransform.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            parentTransform.localScale = s;
        }

        // Bật animation bắn cho khớp hình ảnh
        if (animator != null)
        {
            animator.SetInteger("WeaponType", 3);
            animator.SetTrigger("SimpleBowShot");
        }
    }

    /// <summary>Tính vận tốc phóng (ballistic) để mũi tên tới được điểm aim.</summary>
    private Vector2 BallisticVelocityTo(Vector3 aim)
    {
        float distance = Vector2.Distance(shootPoint.position, aim);
        float t = Mathf.Clamp(distance / axesX, 0.3f, 1.5f);

        float gravityScale = 1.5f;
        Rigidbody2D arrowRb = arrowPrefab != null ? arrowPrefab.GetComponent<Rigidbody2D>() : null;
        if (arrowRb != null) gravityScale = arrowRb.gravityScale;
        float g = -Physics2D.gravity.y * gravityScale;

        float vx = (aim.x - shootPoint.position.x) / t;
        float vy = (aim.y - shootPoint.position.y + 0.5f * g * t * t) / t;
        return new Vector2(vx, vy);
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
