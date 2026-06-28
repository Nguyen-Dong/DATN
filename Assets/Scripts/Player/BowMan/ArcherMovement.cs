using UnityEngine;

/// <summary>
/// Lính cung Player (tầm xa). Cung cấp ngữ cảnh cho <see cref="UnitFSM"/>.
/// Riêng "lính cung hỗ trợ rút lui" (tag RetreatArcher) vẫn dùng hành vi đặc biệt riêng.
/// </summary>
public class ArcherMovement : PlayerMovement, IUnitBrain
{
    protected Archer archer;

    private Transform defendPoint;
    private Transform retreatPoint;

    [Header("Retreat")]
    [Tooltip("Khoảng cách phía sau màn hình để rút lui")]
    [SerializeField] private float retreatOffsetBehindCamera = 3f;

    /// <summary>Đánh dấu đây là lính cung hỗ trợ rút lui (spawn khi bấm Retreat).</summary>
    [HideInInspector] public bool isRetreatArcher = false;

    // Điểm đánh dấu cung thủ yểm trợ sẽ di chuyển tới (do GameCommander gán thẳng khi spawn)
    private Transform retreatStandPoint;
    // Vị trí spawn ban đầu (để biết rút về đâu)
    private Vector3 initialSpawnPos;
    private bool _dbgLogged = false;

    private UnitFSM fsm;

    /// <summary>GameCommander gọi để gán điểm đứng yểm trợ cho cung thủ retreat.</summary>
    public void SetRetreatStandPoint(Transform point) => retreatStandPoint = point;

    protected void Start()
    {
        Load();
        archer = GetComponentInParent<Archer>();

        initialSpawnPos = transform.parent.position;

        if (transform.parent.CompareTag("RetreatArcher"))
            isRetreatArcher = true;

        // Cung thủ có điểm tập hợp phòng thủ RIÊNG (thường lùi sau hàng cận chiến).
        // Tìm "ArcherDefendPoint" trước; nếu chưa tạo thì dùng chung "DefendPoint".
        GameObject dp = GameObject.Find("ArcherDefendPoint");
        if (dp == null) dp = GameObject.Find("DefendPoint");
        if (dp != null) defendPoint = dp.transform;

        GameObject rp = GameObject.Find("RetreatPoint");
        if (rp != null) retreatPoint = rp.transform;

        // Chỉ lính cung thường mới đăng ký đội hình
        if (!isRetreatArcher && UnitFormationManager.Instance != null)
            UnitFormationManager.Instance.RegisterPlayerUnit(transform.parent);

        fsm = new UnitFSM(this);
    }

    void Update()
    {
        if (!archer.CanMove()) return;

        if (isRetreatArcher)
        {
            HandleRetreatArcherBehavior();
            return;
        }

        if (fsm != null) fsm.Tick();
    }

    // ===== IUnitBrain =====

    public Transform Body => transform.parent;
    public bool IsPlayer => true;
    public bool IsAlive => !archer.dead;
    public bool CanAct => archer.CanMove();
    public UnitCommand Command => MapCommand(GameCommander.currentState);
    public bool TargetInVision => archer.attack != null && archer.attack.isEnemyInRange;
    public bool TargetInAttackRange => archer.attack != null && archer.attack.isEnemyInRange;
    public bool EngageInDefend => false; // đứng phòng thủ tại chỗ, vẫn bắn nếu địch vào tầm (ArcherAttack tự xử lý)
    public bool RangedEngage => true;    // tầm xa: đứng yên bắn, không dồn về tuyến cận chiến
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

    // Tầm xa: không truy đuổi, không cần quay theo mục tiêu (đứng bắn về phía trước)
    public void FaceTarget() => Flip(1);
    public void MoveTowardTarget() => StopAndIdle();

    public void StopAndIdle()
    {
        Rigidbody2D rb = transform.parent.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        // LUÔN đưa locomotion về idle khi đứng yên. Việc bắn cung do ArcherAttack quản bằng param riêng
        // (Ready/WeaponType/SimpleBowShot) nên không xung đột; trước đây bỏ qua khiến State kẹt ở 2 -> chạy tại chỗ.
        if (anim != null) anim.SetInteger("State", 0);
    }

    // ===== Hành vi lính cung hỗ trợ rút lui (giữ nguyên) =====

    /// <summary>
    /// - Khi Retreat: đi từ spawnPoint ra ArcherStandPoint rồi đứng bắn.
    /// - Khi chuyển sang Attack/Defend: rút về spawnPoint chờ.
    /// - Bấm Retreat lại: tiến ra ArcherStandPoint. KHÔNG tự hủy.
    /// </summary>
    private void HandleRetreatArcherBehavior()
    {
        if (!_dbgLogged)
        {
            _dbgLogged = true;
            Debug.Log($"<color=#00FF00>[RetreatArcher] standPoint={(retreatStandPoint != null ? "OK @x=" + retreatStandPoint.position.x.ToString("F2") : "NULL")} " +
                      $"| myX={transform.parent.position.x:F2} | speed={speed} | attack={(archer != null && archer.attack != null ? "OK" : "NULL")}</color>");
        }

        if (GameCommander.currentState == GameCommander.CommandState.Retreat)
        {
            // Chưa gán điểm đứng -> đứng yên tại chỗ
            if (retreatStandPoint == null)
            {
                archer.attack.isMovingToPosition = false;
                transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                if (anim != null) anim.SetInteger("State", 0);
                return;
            }

            float targetX = retreatStandPoint.position.x;
            float dist = targetX - transform.parent.position.x;

            if (Mathf.Abs(dist) > 0.3f)
            {
                archer.attack.isMovingToPosition = true;
                directionMove = dist > 0 ? 1f : -1f;
                Flip((int)directionMove);
                Move(directionMove);
            }
            else
            {
                archer.attack.isMovingToPosition = false;
                directionMove = 0f;
                Flip(1);
                transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                if (anim != null) anim.SetInteger("State", 0); // luôn về idle khi đã đứng tại chỗ
            }
        }
        else
        {
            // Quay đầu, di chuyển về spawn (đối xứng - đi đúng hướng dù spawn ở bên nào)
            float dist = initialSpawnPos.x - transform.parent.position.x;

            if (Mathf.Abs(dist) > 0.3f)
            {
                archer.attack.isMovingToPosition = true;
                directionMove = dist > 0 ? 1f : -1f;
                Flip((int)directionMove);
                Move(directionMove);
            }
            else
            {
                archer.attack.isMovingToPosition = false;
                directionMove = 0f;
                Flip(1);
                transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                if (anim != null) anim.SetInteger("State", 0);
            }
        }
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
