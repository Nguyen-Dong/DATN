using UnityEngine;

public class ArcherMovement : PlayerMovement
{
    protected Archer archer;
    protected int oldDirection;

    private Transform defendPoint;
    private Transform retreatPoint;

    [Header("Formation")]
    [Tooltip("Tốc độ lerp về vị trí formation Y")]
    [SerializeField] private float formationLerpSpeed = 8f;

    [Header("Retreat")]
    [Tooltip("Khoảng cách phía sau màn hình để rút lui")]
    [SerializeField] private float retreatOffsetBehindCamera = 3f;

    // Vị trí Y gốc khi unit được spawn (mặt đất)
    private float baseY;

    /// <summary>
    /// Đánh dấu đây là lính cung hỗ trợ rút lui (được spawn khi bấm Retreat).
    /// </summary>
    [HideInInspector] public bool isRetreatArcher = false;

    // Vị trí đích mà lính cung retreat sẽ đứng (ArcherStandPoint)
    private Transform archerStandPoint;

    // Vị trí spawn ban đầu (để biết rút về đâu)
    private Vector3 initialSpawnPos;

    protected void Start()
    {
        Load();
        archer = GetComponentInParent<Archer>();

        // Lưu vị trí Y spawn làm mốc cho formation
        baseY = transform.parent.position.y;

        // Lưu vị trí spawn ban đầu
        initialSpawnPos = transform.parent.position;

        // Kiểm tra nếu parent có tag RetreatArcher
        if (transform.parent.CompareTag("RetreatArcher"))
        {
            isRetreatArcher = true;
        }

        GameObject dp = GameObject.Find("DefendPoint");
        if (dp != null)
        {
            defendPoint = dp.transform;
        }

        GameObject rp = GameObject.Find("RetreatPoint");
        if (rp != null)
        {
            retreatPoint = rp.transform;
        }

        // Tìm ArcherStandPoint cho lính cung retreat
        GameObject asp = GameObject.Find("ArcherStandPoint");
        if (asp != null)
        {
            archerStandPoint = asp.transform;
        }

        // Đăng ký unit vào formation manager (chỉ lính cung thường)
        if (!isRetreatArcher && UnitFormationManager.Instance != null)
        {
            UnitFormationManager.Instance.RegisterPlayerUnit(transform.parent);
        }
    }

    void Update()
    {
        if (!archer.CanMove()) return;

        // Lính cung hỗ trợ rút lui: hành vi riêng
        if (isRetreatArcher)
        {
            HandleRetreatArcherBehavior();
            return;
        }

        // Lính cung thông thường: theo chế độ chỉ huy
        if (GameCommander.currentState == GameCommander.CommandState.Attack)
        {
            HandleAttackState();
        }
        else if (GameCommander.currentState == GameCommander.CommandState.Defend)
        {
            HandleDefendState();
        }
        else if (GameCommander.currentState == GameCommander.CommandState.Retreat)
        {
            HandleRetreatState();
        }
    }

    /// <summary>
    /// Chế độ Tấn công: di chuyển lên trước, khi phát hiện địch bằng raycast thì dừng lại bắn.
    /// ArcherAttack sẽ tự handle việc dừng và bắn khi isEnemyInRange == true.
    /// </summary>
    private void HandleAttackState()
    {
        // Nếu ArcherAttack đã detect enemy -> dừng, không di chuyển
        // ArcherAttack.Update() sẽ tự xử lý dừng + bắn
        if (archer.attack.isEnemyInRange)
        {
            // Đứng yên, quay mặt về phía trước
            directionMove = 0f;
            Flip(1);
            // Không set animation ở đây - ArcherAttack sẽ quản lý
            ApplyFormationOffset();
            return;
        }

        // Không phát hiện địch -> di chuyển về phía trước
        directionMove = 1f;
        Flip(1);
        Move(directionMove);
        ApplyFormationOffset();
    }

    /// <summary>
    /// Chế độ Phòng thủ: tập trung tại DefendPoint và đứng chờ lệnh.
    /// Nếu phát hiện địch trong tầm thì vẫn bắn.
    /// </summary>
    private void HandleDefendState()
    {
        if (defendPoint != null)
        {
            float distToDefend = transform.parent.position.x - defendPoint.position.x;

            if (Mathf.Abs(distToDefend) > 0.3f)
            {
                // Di chuyển về phía DefendPoint
                directionMove = distToDefend > 0 ? -1f : 1f;
                Flip((int)directionMove);
                Move(directionMove);
                ApplyFormationOffset();
                return;
            }
        }

        // Đã đến DefendPoint hoặc không có DefendPoint
        // Đứng yên chờ lệnh, nhưng vẫn bắn nếu thấy địch
        directionMove = 0f;
        Flip(1); // Quay mặt về phía trước
        transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        if (archer.attack.isEnemyInRange)
        {
            // Phát hiện địch -> ArcherAttack tự xử lý bắn
            ApplyFormationOffset();
            return;
        }

        if (anim != null)
        {
            anim.SetInteger("State", 0);
        }
        ApplyFormationOffset();
    }

    /// <summary>
    /// Chế độ Rút lui: lính cung thường cũng rút ra phía sau màn hình.
    /// </summary>
    private void HandleRetreatState()
    {
        float retreatTargetX;

        if (retreatPoint != null)
        {
            retreatTargetX = retreatPoint.position.x;
        }
        else
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float camLeftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;
                retreatTargetX = camLeftEdge - retreatOffsetBehindCamera;
            }
            else
            {
                retreatTargetX = -15f;
            }
        }

        float distToRetreat = transform.parent.position.x - retreatTargetX;

        if (distToRetreat > 0.3f)
        {
            directionMove = -1f;
            Flip(-1);
            Move(directionMove);
        }
        else
        {
            directionMove = 0f;
            transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.SetInteger("State", 0);
            }
        }
    }

    /// <summary>
    /// Hành vi lính cung hỗ trợ rút lui:
    /// - Khi ở chế độ Retreat: di chuyển từ spawnPoint đến ArcherStandPoint, rồi đứng bắn.
    /// - Khi chuyển sang chế độ khác (Attack/Defend): rút về phía sau (spawnPoint) và đứng chờ.
    /// - Khi bấm Retreat lần nữa: lại tiến ra ArcherStandPoint.
    /// KHÔNG tự hủy - lính cung này tồn tại vĩnh viễn.
    /// </summary>
    private void HandleRetreatArcherBehavior()
    {
        if (GameCommander.currentState == GameCommander.CommandState.Retreat)
        {
            // === ĐANG Ở CHẾ ĐỘ RÚT LUI ===
            // Di chuyển đến ArcherStandPoint để tấn công yểm trợ
            float targetX;
            if (archerStandPoint != null)
            {
                targetX = archerStandPoint.position.x;
            }
            else
            {
                // Fallback: tiến lên phía trước một khoảng
                targetX = initialSpawnPos.x + 5f;
            }

            float dist = targetX - transform.parent.position.x;

            if (Mathf.Abs(dist) > 0.3f)
            {
                // Chưa đến vị trí -> đang di chuyển, báo cho ArcherAttack không dừng
                archer.attack.isMovingToPosition = true;
                directionMove = dist > 0 ? 1f : -1f;
                Flip((int)directionMove);
                Move(directionMove);
            }
            else
            {
                // Đã đến ArcherStandPoint -> cho phép ArcherAttack dừng và bắn
                archer.attack.isMovingToPosition = false;
                directionMove = 0f;
                Flip(1); // Quay mặt về phía trước
                transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

                if (archer.attack.isEnemyInRange)
                {
                    // ArcherAttack tự handle bắn
                    return;
                }

                if (anim != null)
                {
                    anim.SetInteger("State", 0);
                }
            }
        }
        else
        {
            // === KHÔNG Ở CHẾ ĐỘ RÚT LUI (Attack/Defend) ===
            // Rút về phía sau (về spawnPoint ban đầu)
            float dist = transform.parent.position.x - initialSpawnPos.x;

            if (dist > 0.3f)
            {
                // Chưa về đến spawnPoint -> đang di chuyển
                archer.attack.isMovingToPosition = true;
                directionMove = -1f;
                Flip(-1);
                Move(directionMove);
            }
            else
            {
                // Đã về spawnPoint -> đứng yên chờ lệnh Retreat tiếp
                archer.attack.isMovingToPosition = false;
                directionMove = 0f;
                Flip(1);
                transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                if (anim != null)
                {
                    anim.SetInteger("State", 0);
                }
            }
        }
    }

    private void Flip(int dir)
    {
        if (dir == 0) return;
        Transform parentTransform = transform.parent;
        Vector3 currentScale = parentTransform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * dir;
        parentTransform.localScale = currentScale;
    }

    /// <summary>
    /// Lerp vị trí Y của unit về đúng slot formation.
    /// </summary>
    private void ApplyFormationOffset()
    {
        if (UnitFormationManager.Instance == null) return;
        if (UnitFormationManager.Instance.GetPlayerUnitCount() <= 1) return;

        Vector2 offset = UnitFormationManager.Instance.GetPlayerFormationOffset(transform.parent);
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

    public void SetAnimator(Animator a)
    {
        anim = a;
    }
}
