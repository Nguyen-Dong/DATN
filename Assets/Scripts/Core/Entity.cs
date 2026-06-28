using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public bool createdDust;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;
    [SerializeField] public bool dead;

    [Tooltip("Phòng thủ: giảm thẳng sát thương nhận vào (tối thiểu vẫn mất 1 máu)")]
    [SerializeField] protected float defense = 0f;

    [Tooltip("Scale ép cho MỌI lính di động khi spawn (đồng nhất player lẫn enemy)")]
    [SerializeField] protected float unitScale = 0.55f;

    [Header("Spawn Jitter (tránh lính đè khít lên nhau)")]
    [Tooltip("Độ lệch X ngẫu nhiên khi spawn")]
    [SerializeField] protected float spawnJitterX = 0.2f;
    [Tooltip("Độ lệch Y ngẫu nhiên khi spawn")]
    [SerializeField] protected float spawnJitterY = 0.4f;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f; // Tắt trọng lực để tránh sụt lún cơ học gây rung giật khi Lerp đội hình Y
            rb.linearVelocity = Vector2.zero; // Xóa vận tốc dư (vd Y âm do 1 bước trọng lực chạy trước Start) -> tránh đi chéo
        }

        // Chỉ áp dụng cho LÍNH DI ĐỘNG (Player layer 7 / Enemy layer 6), KHÔNG áp dụng cho Base (tháp)
        bool isBase = this is PlayerBase || this is EnemyBase;
        bool isMobileUnit = !isBase && (gameObject.layer == 6 || gameObject.layer == 7);

        if (isMobileUnit)
        {
            // Ép scale đồng nhất cho mọi lính (cả player lẫn enemy), giữ dấu để Flip vẫn quay mặt đúng.
            float signX = transform.localScale.x < 0f ? -1f : 1f;
            transform.localScale = new Vector3(unitScale * signX, unitScale, unitScale);

            // Lệch vị trí spawn ngẫu nhiên một chút để các unit không spawn chồng khít 100% lên nhau.
            // Vì FSM không còn chỉnh trục Y -> độ lệch Y được giữ vĩnh viễn, giúp lính luôn tách nhau theo chiều dọc.
            transform.position += new Vector3(
                Random.Range(-spawnJitterX, spawnJitterX),
                Random.Range(-spawnJitterY, spawnJitterY),
                0f);

            // Tách nhẹ unit cùng phe để không đè 100% lên nhau (chạy LateUpdate, tất định, không giật)
            if (GetComponent<UnitSeparation>() == null)
            {
                UnitSeparation sep = gameObject.AddComponent<UnitSeparation>();
                sep.Initialize(1 << gameObject.layer);
            }

            // Thanh máu trên đầu lính
            if (GetComponent<UnitHealthBar>() == null)
            {
                gameObject.AddComponent<UnitHealthBar>();
            }

            // Bỏ qua va chạm vật lý với Base để lính không bị kẹt ở chân tháp.
            //    (Sát thương lên Base vẫn hoạt động vì việc phát hiện dùng OverlapCircle theo layer, không phụ thuộc va chạm vật lý)
            IgnoreCollisionWithBases();
        }
    }

    /// <summary>
    /// Tắt va chạm vật lý giữa lính này và collider của các Base (PlayerBase + EnemyBase).
    /// Tránh lỗi lính bị mắc/kẹt ở chân tháp khi tiến tới gần Base.
    /// </summary>
    private void IgnoreCollisionWithBases()
    {
        Collider2D[] myCols = GetComponentsInChildren<Collider2D>();
        if (myCols == null || myCols.Length == 0) return;

        PlayerBase pb = FindFirstObjectByType<PlayerBase>();
        EnemyBase eb = FindFirstObjectByType<EnemyBase>();

        IgnoreCollisionWith(myCols, pb);
        IgnoreCollisionWith(myCols, eb);
    }

    private void IgnoreCollisionWith(Collider2D[] myCols, Component baseComp)
    {
        if (baseComp == null) return;
        Collider2D[] baseCols = baseComp.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D mc in myCols)
        {
            if (mc == null) continue;
            foreach (Collider2D bc in baseCols)
            {
                if (bc == null) continue;
                Physics2D.IgnoreCollision(mc, bc, true);
            }
        }
    }
    public virtual void TakeDamage(float damage)
    {
        // DEF trừ thẳng, nhưng luôn mất tối thiểu 1 máu mỗi đòn
        float dealt = Mathf.Max(1f, damage - defense);
        currentHealth -= dealt;
        if (currentHealth <= 0)
        {
            EntityDie();
        }
    }

    public void SetDefense(float value) => defense = Mathf.Max(0f, value);
    public float GetDefense() => defense;
    public void TakeHealth(float health)
    {
        currentHealth += health;
    }
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
    }
    public void HealFull()
    {
        currentHealth = maxHealth;
    }
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;

    public virtual void EntityDie()
    {
        dead = true;
        if (gameObject.GetComponent<Collider2D>() != null)
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

}