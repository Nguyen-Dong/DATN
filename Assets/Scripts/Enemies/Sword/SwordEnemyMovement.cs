using UnityEngine;

/// <summary>
/// Lính kiếm Enemy (cận chiến). Cung cấp ngữ cảnh phe Địch cho <see cref="UnitFSM"/>.
/// Phát hiện player bằng TẦM NHÌN (vòng tròn quanh thân) -> chọn gần nhất -> áp sát rồi chém.
/// Lệnh toàn quân lấy từ <see cref="EnemyAI"/>.
/// </summary>
public class SwordEnemyMovement : EnemyMovement, IUnitBrain
{
    protected SwordEnemy sword_E;

    private Transform defendPoint;
    private Transform retreatPoint;
    private EnemyBase enemyBase;

    [Header("Tầm nhìn (giao tranh)")]
    [Tooltip("Bán kính phát hiện player để lao vào đánh (nên ~3-4 lần tầm chém)")]
    [SerializeField] private float visionRange = 2.5f;

    private UnitFSM fsm;
    private ContactFilter2D visionFilter;
    private Transform currentTarget;

    protected void Start()
    {
        Load();
        sword_E = GetComponentInParent<SwordEnemy>();
        enemyBase = FindFirstObjectByType<EnemyBase>();

        if (UnitFormationManager.Instance != null)
            UnitFormationManager.Instance.RegisterEnemyUnit(transform.parent);

        GameObject dp = GameObject.Find("EnemyDefendPoint");
        if (dp != null) defendPoint = dp.transform;

        GameObject rp = GameObject.Find("EnemyRetreatPoint");
        if (rp != null) retreatPoint = rp.transform;

        // Tầm nhìn lọc theo phe ĐỊCH của enemy = Player
        visionFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = false
        };
        visionFilter.SetLayerMask(LayerMask.GetMask("Player"));
        visionRange *= Random.Range(0.9f, 1.1f);

        fsm = new UnitFSM(this);
    }

    void Update()
    {
        currentTarget = UnitTargeting.FindNearest(transform.parent.position, visionRange, visionFilter, transform.parent);
        if (fsm != null) fsm.Tick();
    }

    // ===== IUnitBrain =====

    public Transform Body => transform.parent;
    public bool IsPlayer => false;
    public bool IsAlive => !sword_E.dead;
    public bool CanAct => sword_E.CanMove();
    public UnitCommand Command => MapCommand();
    public bool TargetInVision => currentTarget != null;
    public bool TargetInAttackRange => sword_E.attack != null && sword_E.attack.isPlayerInRange;
    public bool EngageInDefend => true;  // địch lao ra đánh ngay khi thấy player (kể cả đang thủ), trừ khi đang Rút lui
    public bool RangedEngage => false;   // cận chiến
    public bool HasDefendPost => true;   // luôn có (điểm phòng thủ hoặc tính từ Base)
    public bool UsesDefendFormation => true; // phòng thủ thì xếp thành cột dọc 4 con
    public float DefendAnchorX => ComputeDefendX();
    public float RetreatTargetX => ComputeRetreatX();
    public int ForwardSign => -1;        // địch tiến sang trái (phía Player)

    public void MoveStep(float dir)
    {
        Flip(dir >= 0f ? 1 : -1);
        Move(dir);
    }

    public void FaceForward() => Flip(-1);

    public void FaceTarget()
    {
        if (currentTarget == null) { Flip(-1); return; }
        Flip(currentTarget.position.x >= transform.parent.position.x ? 1 : -1);
    }

    public void MoveTowardTarget()
    {
        if (currentTarget == null) { StopAndIdle(); return; }

        Rigidbody2D rb = transform.parent.GetComponent<Rigidbody2D>();
        Vector2 to = (Vector2)currentTarget.position - (Vector2)transform.parent.position;
        // Vận tốc chuẩn hóa theo hướng tới mục tiêu -> tốc độ KHÔNG đổi dù đi chéo hay thẳng
        Vector2 dir = to.sqrMagnitude > 0.0001f ? to.normalized : Vector2.zero;
        if (rb != null) rb.linearVelocity = dir * speed;
        Flip(dir.x >= 0f ? 1 : -1);
        if (anim != null) anim.SetInteger("State", 2);
    }

    public void StopAndIdle()
    {
        Rigidbody2D rb = transform.parent.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero; // zero cả Y để không trôi sau khi dừng/chém
        if (anim != null) anim.SetInteger("State", 0);
    }

    // ===== Helpers =====

    private float ComputeDefendX()
    {
        if (defendPoint != null) return defendPoint.position.x;
        if (enemyBase != null) return enemyBase.transform.position.x - 5f;
        return 8f;
    }

    private float ComputeRetreatX()
    {
        if (retreatPoint != null) return retreatPoint.position.x;
        if (enemyBase != null) return enemyBase.transform.position.x + 3f;
        return 15f;
    }

    private UnitCommand MapCommand()
    {
        if (EnemyAI.Instance == null) return UnitCommand.Defend;
        switch (EnemyAI.Instance.currentEnemyState)
        {
            case EnemyAI.EnemyCommandState.Attack: return UnitCommand.Attack;
            case EnemyAI.EnemyCommandState.Retreat: return UnitCommand.Retreat;
            default: return UnitCommand.Defend;
        }
    }

    private void Flip(int dir)
    {
        if (dir == 0) return;
        Vector3 s = transform.parent.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.parent.localScale = s;
    }
}
