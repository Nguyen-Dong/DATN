using UnityEngine;

/// <summary>
/// Lính kiếm Player (cận chiến). Cung cấp ngữ cảnh cho <see cref="UnitFSM"/>.
/// Phát hiện địch bằng TẦM NHÌN (vòng tròn quanh thân) -> chọn địch gần nhất -> áp sát rồi chém.
/// </summary>
public class SwordMovement : PlayerMovement, IUnitBrain
{
    protected Sword sword;

    private Transform defendPoint;
    private Transform retreatPoint;

    [Header("Tầm nhìn (giao tranh)")]
    [Tooltip("Bán kính phát hiện địch để lao vào đánh (nên ~3-4 lần tầm chém)")]
    [SerializeField] private float visionRange = 2.5f;

    [Header("Retreat")]
    [Tooltip("Khoảng cách phía sau màn hình để rút lui (tính từ mép trái camera)")]
    [SerializeField] private float retreatOffsetBehindCamera = 3f;

    private UnitFSM fsm;
    private ContactFilter2D visionFilter;
    private Transform currentTarget;

    protected void Start()
    {
        Load();
        sword = GetComponentInParent<Sword>();

        // Giữ nguyên scale của prefab (không ép 0.55 nữa) - thống nhất với lính cung

        GameObject dp = GameObject.Find("DefendPoint");
        if (dp != null) defendPoint = dp.transform;

        GameObject rp = GameObject.Find("RetreatPoint");
        if (rp != null) retreatPoint = rp.transform;

        if (UnitFormationManager.Instance != null)
            UnitFormationManager.Instance.RegisterPlayerUnit(transform.parent);

        // Tầm nhìn lọc theo phe ĐỊCH (Enemy)
        visionFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = false
        };
        visionFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        visionRange *= Random.Range(0.9f, 1.1f); // lệch nhẹ để không trụ chồng 1 điểm

        fsm = new UnitFSM(this);
    }

    void Update()
    {
        // Quét mục tiêu gần nhất trong tầm nhìn (1 lần/frame)
        currentTarget = UnitTargeting.FindNearest(transform.parent.position, visionRange, visionFilter, transform.parent);
        if (fsm != null) fsm.Tick();
    }

    // ===== IUnitBrain =====

    public Transform Body => transform.parent;
    public bool IsPlayer => true;
    public bool IsAlive => !sword.dead;
    public bool CanAct => sword.CanMove();
    public UnitCommand Command => MapCommand(GameCommander.currentState);
    public bool TargetInVision => currentTarget != null;
    public bool TargetInAttackRange => sword.attack != null && sword.attack.isEnemyInRange;
    public bool EngageInDefend => false; // Player chỉ lao ra đánh khi đang ở lệnh Tấn công
    public bool RangedEngage => false;   // cận chiến
    public bool HasDefendPost => defendPoint != null;
    public bool UsesDefendFormation => false;
    public float DefendAnchorX => defendPoint != null ? defendPoint.position.x : transform.parent.position.x;
    public float RetreatTargetX => ComputeRetreatX();
    public int ForwardSign => 1;

    public void MoveStep(float dir)
    {
        Flip(dir >= 0f ? 1 : -1);
        Move(dir);
    }

    public void FaceForward() => Flip(1);

    public void FaceTarget()
    {
        if (currentTarget == null) { Flip(1); return; }
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

    private float ComputeRetreatX()
    {
        if (retreatPoint != null) return retreatPoint.position.x;
        Camera cam = Camera.main;
        if (cam != null)
            return cam.transform.position.x - cam.orthographicSize * cam.aspect - retreatOffsetBehindCamera;
        return -15f;
    }

    private static UnitCommand MapCommand(GameCommander.CommandState s)
    {
        switch (s)
        {
            case GameCommander.CommandState.Attack: return UnitCommand.Attack;
            case GameCommander.CommandState.Retreat: return UnitCommand.Retreat;
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

    public void SetAnimator(Animator a) => anim = a;
}
