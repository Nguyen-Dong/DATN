using UnityEngine;

/// <summary>
/// Buff chỉ số TẠM THỜI (kéo dài duration) gắn lên 1 đơn vị: +ATK và/hoặc +DEF, kèm VFX.
/// Hết giờ tự hoàn lại chỉ số và hủy. Có thể chồng nhiều buff (mỗi buff tự quản delta riêng).
/// </summary>
public class SkillBuff : MonoBehaviour
{
    private EntityAttack atk;
    private Entity ent;
    private float atkDelta;
    private float defDelta;
    private float timeLeft;
    private GameObject vfxInstance;

    public void Begin(float atkAmount, float defAmount, float duration, GameObject vfxPrefab)
    {
        atk = GetComponentInChildren<EntityAttack>();
        ent = GetComponent<Entity>();
        atkDelta = atkAmount;
        defDelta = defAmount;
        timeLeft = duration;

        if (atk != null) atk.damage += atkDelta;
        if (ent != null && defDelta != 0f) ent.SetDefense(ent.GetDefense() + defDelta);

        if (vfxPrefab != null)
            vfxInstance = Instantiate(vfxPrefab, transform); // bật VFX trên đơn vị
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) End();
    }

    private void End()
    {
        if (atk != null) atk.damage -= atkDelta;
        if (ent != null && defDelta != 0f) ent.SetDefense(ent.GetDefense() - defDelta);
        if (vfxInstance != null) Destroy(vfxInstance);
        Destroy(this);
    }

    private void OnDestroy()
    {
        if (vfxInstance != null) Destroy(vfxInstance);
    }
}
