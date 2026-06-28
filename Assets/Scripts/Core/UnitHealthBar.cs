using UnityEngine;

/// <summary>
/// Thanh máu world-space hiển thị trên đầu lính.
/// Được sinh hoàn toàn bằng code (không cần prefab/sprite) nên chỉ cần AddComponent là chạy.
/// Holder của thanh máu là một GameObject ĐỘC LẬP (không phải con của lính) để không bị
/// lật/biến dạng theo việc Flip (localScale.x âm) của lính.
/// </summary>
public class UnitHealthBar : MonoBehaviour
{
    [Header("Kích thước (world units)")]
    [SerializeField] private float width = 0.7f;
    [SerializeField] private float height = 0.12f;

    [Tooltip("Khoảng cách thêm phía trên đỉnh đầu lính")]
    [SerializeField] private float headPadding = 0.15f;

    [Tooltip("Thứ tự vẽ (cao hơn để nổi trên sprite lính)")]
    [SerializeField] private int sortingOrder = 100;

    [Tooltip("Ẩn thanh máu khi đầy máu")]
    [SerializeField] private bool hideWhenFull = true;

    private Entity entity;
    private Transform holder;
    private Transform fill;
    private SpriteRenderer fillRenderer;
    private float headOffset;

    // Sprite trắng 1x1 dùng chung cho mọi thanh máu
    private static Sprite _whiteSprite;

    private void Start()
    {
        entity = GetComponent<Entity>();
        headOffset = ComputeHeadOffset();
        BuildBar();
    }

    private static Sprite GetWhiteSprite()
    {
        if (_whiteSprite == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            _whiteSprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                tex.width); // pixelsPerUnit = width -> sprite = 1 world unit
        }
        return _whiteSprite;
    }

    /// <summary>
    /// Tính chiều cao từ gốc lính tới đỉnh đầu.
    /// Ưu tiên dùng bounds của Collider2D (thân) để NHẤT QUÁN giữa các loại lính — tránh bị đẩy lên cao
    /// do các sprite phụ (vũ khí giơ cao, bộ phận...) như ở skeleton enemy.
    /// </summary>
    private float ComputeHeadOffset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) col = GetComponentInChildren<Collider2D>();
        if (col != null)
            return (col.bounds.max.y - transform.position.y) + headPadding;

        // Fallback: nếu không có collider, dùng bounds sprite
        SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();
        if (rends == null || rends.Length == 0)
            return 1.0f;

        float maxY = transform.position.y;
        bool found = false;
        foreach (SpriteRenderer r in rends)
        {
            if (r == null) continue;
            maxY = Mathf.Max(maxY, r.bounds.max.y);
            found = true;
        }
        if (!found) return 1.0f;
        return (maxY - transform.position.y) + headPadding;
    }

    private void BuildBar()
    {
        Sprite sp = GetWhiteSprite();

        GameObject holderGO = new GameObject(gameObject.name + "_HealthBar");
        holder = holderGO.transform;

        // Nền (viền tối)
        GameObject bgGO = new GameObject("BG");
        bgGO.transform.SetParent(holder, false);
        SpriteRenderer bgRenderer = bgGO.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = sp;
        bgRenderer.color = new Color(0f, 0f, 0f, 0.65f);
        bgRenderer.sortingOrder = sortingOrder;
        bgGO.transform.localScale = new Vector3(width, height, 1f);

        // Phần máu (xanh -> đỏ), neo về bên trái
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(holder, false);
        fillRenderer = fillGO.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = sp;
        fillRenderer.color = Color.green;
        fillRenderer.sortingOrder = sortingOrder + 1;
        fill = fillGO.transform;

        UpdateBar();
    }

    private void LateUpdate()
    {
        if (holder == null) return;

        // Lính chết -> bỏ thanh máu
        if (entity == null || entity.dead)
        {
            Destroy(holder.gameObject);
            holder = null;
            return;
        }

        UpdateBar();
    }

    private void UpdateBar()
    {
        if (holder == null) return;

        float max = entity.GetMaxHealth();
        float cur = Mathf.Clamp(entity.GetCurrentHealth(), 0f, max);
        float ratio = max > 0f ? cur / max : 0f;

        bool show = !(hideWhenFull && ratio >= 0.999f);
        holder.gameObject.SetActive(show);

        // Vị trí: bám trên đầu lính, luôn thẳng đứng (không lật theo lính)
        Vector3 pos = transform.position;
        pos.y += headOffset;
        holder.position = pos;
        holder.rotation = Quaternion.identity;

        if (!show) return;

        // Màu chuyển từ đỏ (ít máu) sang xanh (đầy máu)
        fillRenderer.color = Color.Lerp(Color.red, Color.green, ratio);

        // Neo trái: thu hẹp bề rộng theo tỉ lệ máu và dịch sang trái
        float w = width * ratio;
        fill.localScale = new Vector3(w, height, 1f);
        fill.localPosition = new Vector3(-(width - w) * 0.5f, 0f, 0f);
    }

    private void OnDestroy()
    {
        if (holder != null)
            Destroy(holder.gameObject);
    }
}
