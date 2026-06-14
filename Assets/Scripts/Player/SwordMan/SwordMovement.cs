using Unity.VisualScripting;
using UnityEngine;

public class SwordMovement : PlayerMovement
{
    protected Sword sword;
    protected int oldDirection;

    private Transform defendPoint;
    private Transform retreatPoint;

    [Header("Formation")]
    [Tooltip("Khoảng cách giữ phía sau unit tiền tuyến khi chưa đến lượt đánh")]
    [SerializeField] private float formationHoldDistance = 1.0f;

    [Tooltip("Tốc độ lerp về vị trí formation Y (càng cao càng nhanh)")]
    [SerializeField] private float formationLerpSpeed = 8f;

    [Header("Retreat")]
    [Tooltip("Khoảng cách phía sau màn hình để rút lui (tính từ camera left edge)")]
    [SerializeField] private float retreatOffsetBehindCamera = 3f;

    // Vị trí Y gốc khi unit được spawn (mặt đất)
    private float baseY;

    [Header("Scale")]
    [Tooltip("Scale của lính cận chiến (phải bằng enemy)")]
    [SerializeField] private float unitScale = 0.55f;

    protected void Start()
    {
        Load();
        sword = GetComponentInParent<Sword>();

        // Đặt scale bằng enemy
        transform.parent.localScale = new Vector3(unitScale, unitScale, unitScale);

        // Lưu vị trí Y spawn làm mốc cho formation
        baseY = transform.parent.position.y;

        GameObject dp = GameObject.Find("DefendPoint");
        if (dp != null)
        {
            defendPoint = dp.transform;
        }

        // Tìm RetreatPoint (nếu có trong scene)
        GameObject rp = GameObject.Find("RetreatPoint");
        if (rp != null)
        {
            retreatPoint = rp.transform;
        }

        // Đăng ký unit vào formation manager
        if (UnitFormationManager.Instance != null)
        {
            UnitFormationManager.Instance.RegisterPlayerUnit(transform.parent);
        }
    }
    void Update()
    {
        if (!sword.CanMove()) return;

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
    /// Chế độ Tấn công: di chuyển về phía trước, đánh khi gặp địch.
    /// </summary>
    private void HandleAttackState()
    {
        // Kiểm tra row trong formation
        int myRow = 0;
        if (UnitFormationManager.Instance != null)
        {
            myRow = UnitFormationManager.Instance.GetPlayerRow(transform.parent);
        }

        if (sword.attack.DetectEnemy())
        {
            // Dừng lại và đánh (cả hàng đầu lẫn hàng sau)
            transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.SetInteger("State", 0);
            }
            return;
        }

        // Không thấy địch -> di chuyển về phía trước
        directionMove = 1f;
        Flip(1);
        Move(directionMove);
        ApplyFormationOffset();
    }

    /// <summary>
    /// Chế độ Phòng thủ: tập trung tại DefendPoint và đứng chờ lệnh.
    /// Lính sẽ di chuyển về DefendPoint nếu đang ở xa, khi đến nơi thì đứng yên.
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
                return;
            }
        }

        // Đã đến DefendPoint hoặc không có DefendPoint -> đứng yên chờ lệnh
        directionMove = 0f;
        Flip(1); // Quay mặt về phía trước (phải)
        transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        if (anim != null)
        {
            anim.SetInteger("State", 0);
        }
        ApplyFormationOffset();
    }

    /// <summary>
    /// Chế độ Rút lui: tất cả lính kiếm rút ra phía sau màn hình.
    /// Di chuyển về bên trái (phía sau) cho đến khi ra ngoài màn hình.
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
            // Tính vị trí phía sau màn hình dựa trên camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                float camLeftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;
                retreatTargetX = camLeftEdge - retreatOffsetBehindCamera;
            }
            else
            {
                retreatTargetX = -15f; // Fallback
            }
        }

        float distToRetreat = transform.parent.position.x - retreatTargetX;

        if (distToRetreat > 0.3f)
        {
            // Vẫn chưa đến vị trí rút lui -> di chuyển về bên trái
            directionMove = -1f;
            Flip(-1);
            Move(directionMove);
        }
        else
        {
            // Đã đến vị trí rút lui -> đứng yên
            directionMove = 0f;
            transform.parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.SetInteger("State", 0);
            }
        }
    }

    /// <summary>
    /// Lerp vị trí Y của unit về đúng slot formation.
    /// Dùng position lerp thay vì AddForce để ổn định, không bị bay.
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

    private void Flip(int dir)
    {
        if (dir == 0) return;
        Transform parentTranform = transform.parent;
        Vector3 currentScale = parentTranform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * dir;
        parentTranform.localScale = currentScale;
    }

    public void SetAnimator(Animator a)
    {
        anim = a;
    }
}
