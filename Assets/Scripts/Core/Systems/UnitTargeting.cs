using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tiện ích tìm mục tiêu (Entity còn sống) GẦN NHẤT trong một bán kính, lọc theo LayerMask phe địch.
/// Dùng List tái sử dụng + ContactFilter2D nên không cấp phát rác mỗi frame.
/// </summary>
public static class UnitTargeting
{
    private static readonly List<Collider2D> _hits = new List<Collider2D>(32);

    public static Transform FindNearest(Vector2 from, float range, ContactFilter2D filter, Transform self)
    {
        int n = Physics2D.OverlapCircle(from, range, filter, _hits);
        Transform best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < n; i++)
        {
            Collider2D c = _hits[i];
            if (c == null) continue;

            Entity e = c.GetComponent<Entity>();
            if (e == null) e = c.GetComponentInParent<Entity>();
            if (e == null || e.dead) continue;
            if (e.transform == self) continue;
            // KHÔNG nhắm Nhà (Base) bằng tầm nhìn -> tránh cả đám dồn về tâm collider lớn của Nhà.
            // Nhà sẽ được xử lý qua tầm đánh (lính cứ tiến thẳng tới rồi chém khi vào tầm).
            if (e is PlayerBase || e is EnemyBase) continue;

            float d = ((Vector2)e.transform.position - from).sqrMagnitude;
            if (d < bestSqr)
            {
                bestSqr = d;
                best = e.transform;
            }
        }
        return best;
    }
}
