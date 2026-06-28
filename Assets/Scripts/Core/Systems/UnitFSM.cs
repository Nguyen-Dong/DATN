using UnityEngine;

/// <summary>Lệnh chiến thuật toàn quân (thống nhất cho cả Player lẫn Enemy).</summary>
public enum UnitCommand { Defend, Attack, Retreat }

/// <summary>Trạng thái hành vi của TỪNG lính (do UnitFSM sở hữu).</summary>
public enum UnitState { Advancing, MovingToPost, Holding, Engaging, Retreating, Dead }

/// <summary>
/// Hợp đồng mà mỗi loại lính (Sword/Archer/SwordEnemy...) cung cấp cho <see cref="UnitFSM"/>.
/// Tách phần "phe & loại" (di chuyển, animation, phát hiện địch, mốc phòng thủ/rút lui) ra khỏi
/// phần "quyết định trạng thái" để logic trạng thái chỉ viết MỘT lần, dùng chung 2 phe.
/// </summary>
public interface IUnitBrain
{
    /// <summary>Transform gốc của lính (chứa Rigidbody2D/Entity). Lưu ý: script movement nằm ở GameObject con.</summary>
    Transform Body { get; }

    bool IsPlayer { get; }
    /// <summary>Còn sống (chưa chết).</summary>
    bool IsAlive { get; }
    /// <summary>Có thể hành động (không chết, không bị ngã/stun).</summary>
    bool CanAct { get; }

    /// <summary>Lệnh toàn quân hiện tại của phe này.</summary>
    UnitCommand Command { get; }
    /// <summary>Có địch trong TẦM NHÌN không (rộng hơn tầm đánh) -> nên lao vào giao tranh.</summary>
    bool TargetInVision { get; }
    /// <summary>Địch đã vào TẦM ĐÁNH chưa -> dừng lại để script Attack ra đòn.</summary>
    bool TargetInAttackRange { get; }
    /// <summary>Áp sát mục tiêu gần nhất (cả X lẫn Y) - dùng cho cận chiến.</summary>
    void MoveTowardTarget();
    /// <summary>Quay mặt về phía mục tiêu.</summary>
    void FaceTarget();

    /// <summary>Khi đang Phòng thủ mà thấy địch thì có lao ra giao tranh không (Enemy = true, Player = false).</summary>
    bool EngageInDefend { get; }
    /// <summary>Giao tranh kiểu tầm xa: đứng tại chỗ bắn (Archer = true) thay vì dồn về tuyến cận chiến.</summary>
    bool RangedEngage { get; }

    bool HasDefendPost { get; }
    /// <summary>Khi Phòng thủ thì xếp thành đội hình (cột dọc 4 con) thay vì đứng dồn 1 chỗ. (Enemy = true)</summary>
    bool UsesDefendFormation { get; }
    /// <summary>X mốc của điểm phòng thủ (chưa cộng offset đội hình).</summary>
    float DefendAnchorX { get; }
    /// <summary>X đích khi rút lui.</summary>
    float RetreatTargetX { get; }
    /// <summary>Hướng tiến của phe: +1 (Player sang phải), -1 (Enemy sang trái).</summary>
    int ForwardSign { get; }

    /// <summary>Đi bộ theo hướng dir (kèm quay mặt + animation chạy).</summary>
    void MoveStep(float dir);
    /// <summary>Quay mặt về hướng tiến.</summary>
    void FaceForward();
    /// <summary>Dừng lại + animation đứng yên (loại tầm xa có thể để script Attack tự quản animation).</summary>
    void StopAndIdle();
}

/// <summary>
/// Máy trạng thái hành vi dùng chung cho mọi lính. Mỗi frame đọc (lệnh toàn quân + phát hiện địch)
/// rồi quyết định trạng thái và ra lệnh cho lính qua <see cref="IUnitBrain"/>.
/// Việc giữ vị trí đội hình (X theo hàng + Y trong hàng) ủy thác cho <see cref="UnitFormationManager"/>.
/// </summary>
public class UnitFSM
{
    private readonly IUnitBrain brain;
    // Lệch X ngẫu nhiên CỐ ĐỊNH cho riêng unit này -> mỗi lính trụ ở điểm hơi khác nhau quanh mốc, không dồn 1 chỗ.
    private readonly float defendJitterX;
    public UnitState State { get; private set; } = UnitState.Holding;

    public UnitFSM(IUnitBrain brain)
    {
        this.brain = brain;
        defendJitterX = Random.Range(-0.35f, 0.35f);
    }

    public void Tick()
    {
        if (!brain.IsAlive) { State = UnitState.Dead; return; }
        if (!brain.CanAct) return;

        UnitCommand cmd = brain.Command;

        // Rút lui ưu tiên cao nhất
        if (cmd == UnitCommand.Retreat) { Retreat(); return; }

        // Giao tranh khi: thấy địch (lính) trong TẦM NHÌN, HOẶC có gì đó (kể cả Nhà) đã vào TẦM ĐÁNH.
        if ((brain.TargetInVision || brain.TargetInAttackRange) && (cmd == UnitCommand.Attack || brain.EngageInDefend))
        {
            Engage();
            return;
        }

        if (cmd == UnitCommand.Attack) { Advance(); return; }

        Defend();
    }

    // ===== CÁC TRẠNG THÁI =====
    // (Đã gỡ toàn bộ logic định vị đội hình/giãn hàng. Lính chỉ di chuyển/dừng/đánh tại chỗ.
    //  Việc tránh đè sẽ được viết lại đơn giản hơn sau.)

    private void Advance()
    {
        State = UnitState.Advancing;
        brain.MoveStep(brain.ForwardSign);
    }

    private void Engage()
    {
        State = UnitState.Engaging;

        if (brain.RangedEngage)
        {
            // Tầm xa: đứng tại chỗ bắn
            brain.FaceTarget();
            brain.StopAndIdle();
            return;
        }

        // Cận chiến:
        // - Đã trong tầm đánh (kể cả Nhà) -> dừng tại chỗ chém, KHÔNG đuổi/dồn.
        // - Chưa tới tầm đánh (chỉ có lính trong tầm nhìn) -> áp sát mục tiêu gần nhất.
        if (brain.TargetInAttackRange)
        {
            brain.FaceTarget();
            brain.StopAndIdle();
        }
        else
        {
            brain.MoveTowardTarget();
        }
    }

    private void Defend()
    {
        // Phòng thủ theo ĐỘI HÌNH (enemy): đi ra xếp thành cột dọc 4 con
        if (brain.UsesDefendFormation && brain.HasDefendPost && UnitFormationManager.Instance != null)
        {
            DefendInFormation();
            return;
        }

        // Phòng thủ đơn giản (player): đi tới điểm thủ (lệch ngẫu nhiên) rồi đứng yên
        if (brain.HasDefendPost)
        {
            float dist = brain.Body.position.x - (brain.DefendAnchorX + defendJitterX);
            if (Mathf.Abs(dist) > 0.3f)
            {
                State = UnitState.MovingToPost;
                brain.MoveStep(dist > 0f ? -1f : 1f);
                return;
            }
        }

        State = UnitState.Holding;
        brain.FaceForward();
        brain.StopAndIdle();
    }

    private void DefendInFormation()
    {
        Transform body = brain.Body;
        Vector2 slot = UnitFormationManager.Instance.GetEnemyFormationSlot(body, brain.DefendAnchorX);

        float dx = body.position.x - slot.x;
        if (Mathf.Abs(dx) > 0.3f)
        {
            // Còn xa cột -> ĐI THẲNG NGANG tới cột (giữ nguyên Y, không kéo chéo)
            State = UnitState.MovingToPost;
            brain.MoveStep(dx > 0f ? -1f : 1f);
            return;
        }

        // Đã tới cột -> đứng yên, mới lerp khít vào slot (cả X lẫn Y vào đúng hàng)
        State = UnitState.Holding;
        brain.FaceForward();
        brain.StopAndIdle();
        Vector3 p = body.position;
        p.x = Mathf.Lerp(p.x, slot.x, Time.deltaTime * 8f);
        p.y = Mathf.Lerp(p.y, slot.y, Time.deltaTime * 8f);
        body.position = p;
    }

    private void Retreat()
    {
        float dist = Mathf.Abs(brain.Body.position.x - brain.RetreatTargetX);
        if (dist > 0.3f)
        {
            State = UnitState.Retreating;
            brain.MoveStep(-brain.ForwardSign); // lùi theo hướng ngược hướng tiến
            return;
        }

        State = UnitState.Holding;
        brain.FaceForward();
        brain.StopAndIdle();
    }
}
