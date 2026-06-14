using UnityEngine;

public class SwordEnemyMovement : EnemyMovement
{
    protected SwordEnemy sword_E;
    protected int oldDirection;

    [Header("Formation")]
    [Tooltip("Tốc độ lerp về vị trí formation Y")]
    [SerializeField] private float formationLerpSpeed = 8f;

    // Vị trí Y gốc khi unit được spawn (mặt đất)
    private float baseY;

    private Transform defendPoint;
    private Transform retreatPoint;

    protected void Start()
    {
        Load();
        sword_E = GetComponentInParent<SwordEnemy>();

        // Lưu vị trí Y spawn làm mốc cho formation
        baseY = transform.parent.position.y;

        // Đăng ký enemy unit vào formation manager
        if (UnitFormationManager.Instance != null)
        {
            UnitFormationManager.Instance.RegisterEnemyUnit(transform.parent);
        }

        // Tìm điểm phòng thủ & rút lui của Địch trong Scene
        GameObject dp = GameObject.Find("EnemyDefendPoint");
        if (dp != null)
        {
            defendPoint = dp.transform;
        }

        GameObject rp = GameObject.Find("EnemyRetreatPoint");
        if (rp != null)
        {
            retreatPoint = rp.transform;
        }
    }

    void Update()
    {
        if (!sword_E.CanMove()) return;

        // Lấy trạng thái chiến thuật hiện tại từ AI
        EnemyAI.EnemyCommandState aiState = EnemyAI.EnemyCommandState.Defend;
        if (EnemyAI.Instance != null)
        {
            aiState = EnemyAI.Instance.currentEnemyState;
        }

        // Nếu có người chơi ở tầm đánh và KHÔNG ở trạng thái Rút lui -> Dừng lại tấn công
        if (sword_E.attack.DetectPlayer() && aiState != EnemyAI.EnemyCommandState.Retreat)
        {
            transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.SetInteger("State", 0);
            }
            ApplyFormationOffset();
            return;
        }

        // Thực thi di chuyển theo trạng thái của AI
        if (aiState == EnemyAI.EnemyCommandState.Attack)
        {
            HandleAttackState();
        }
        else if (aiState == EnemyAI.EnemyCommandState.Defend)
        {
            HandleDefendState();
        }
        else if (aiState == EnemyAI.EnemyCommandState.Retreat)
        {
            HandleRetreatState();
        }
    }

    /// <summary>
    /// Chế độ Tấn công: Tiến về bên trái (phía người chơi).
    /// </summary>
    private void HandleAttackState()
    {
        directionMove = -1; // Di chuyển sang trái
        Flip(-1); // Quay mặt sang trái
        Move(directionMove);
        ApplyFormationOffset();
    }

    /// <summary>
    /// Chế độ Phòng thủ: Tập trung đứng tấn thủ tại EnemyDefendPoint.
    /// </summary>
    private void HandleDefendState()
    {
        float targetX;
        if (defendPoint != null)
        {
            targetX = defendPoint.position.x;
        }
        else
        {
            // Tự động tính toán vị trí cách EnemyBase 5 đơn vị về phía bên trái (hướng sang phía Player)
            EnemyBase eBase = FindObjectOfType<EnemyBase>();
            if (eBase != null)
            {
                targetX = eBase.transform.position.x - 5f;
            }
            else
            {
                targetX = 8f; // Fallback mặc định
            }
        }

        float distToDefend = transform.parent.position.x - targetX;

        if (Mathf.Abs(distToDefend) > 0.3f)
        {
            // Di chuyển về phía DefendPoint
            directionMove = distToDefend > 0 ? -1 : 1;
            Flip(directionMove);
            Move(directionMove);
        }
        else
        {
            // Đã đến vị trí phòng thủ -> Đứng yên chờ
            directionMove = 0;
            Flip(-1); // Quay mặt về phía trước (trái)
            transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.SetInteger("State", 0);
            }
        }
        ApplyFormationOffset();
    }

    /// <summary>
    /// Chế độ Rút lui: Toàn quân địch quay đầu chạy về bên phải (Base địch).
    /// </summary>
    private void HandleRetreatState()
    {
        float targetX;
        if (retreatPoint != null)
        {
            targetX = retreatPoint.position.x;
        }
        else
        {
            // Tự động tính toán vị trí lùi sâu phía sau Base địch 3 đơn vị
            EnemyBase eBase = FindObjectOfType<EnemyBase>();
            if (eBase != null)
            {
                targetX = eBase.transform.position.x + 3f;
            }
            else
            {
                targetX = 15f; // Fallback mặc định
            }
        }

        float distToRetreat = targetX - transform.parent.position.x;

        if (distToRetreat > 0.3f)
        {
            // Vẫn chưa về đến điểm đích rút lui -> Tiếp tục chạy về bên phải
            directionMove = 1;
            Flip(1); // Quay mặt sang phải (hướng chạy trốn)
            Move(directionMove);
        }
        else
        {
            // Đã về đến nơi an toàn -> Đứng yên
            directionMove = 0;
            Flip(-1); // Quay mặt lại sẵn sàng chiến đấu (quay sang trái)
            transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.SetInteger("State", 0);
            }
        }
        ApplyFormationOffset();
    }

    private void Flip(int dir)
    {
        if (dir == 0) return;
        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            Vector3 currentScale = parentTransform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * dir;
            parentTransform.localScale = currentScale;
        }
    }

    /// <summary>
    /// Lerp vị trí Y của unit về đúng slot formation.
    /// </summary>
    private void ApplyFormationOffset()
    {
        if (UnitFormationManager.Instance == null) return;
        if (UnitFormationManager.Instance.GetEnemyUnitCount() <= 1) return;

        Vector2 offset = UnitFormationManager.Instance.GetEnemyFormationOffset(transform.parent);
        float targetY = baseY + offset.y;

        // Lerp position Y về đúng vị trí slot
        Vector3 pos = transform.parent.position;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * formationLerpSpeed);
        transform.parent.position = pos;

        // Zero vel.y để gravity không kéo unit khỏi vị trí formation
        Rigidbody2D rb = transform.parent.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }
}
