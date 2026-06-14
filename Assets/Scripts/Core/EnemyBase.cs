using UnityEngine;

public class EnemyBase : Enemy
{
    [Header("Regeneration (Configurable)")]
    [Tooltip("Thời gian chờ không nhận sát thương để bắt đầu hồi phục (giây)")]
    [SerializeField] private float healDelay = 5f;
    [Tooltip("Lượng máu hồi phục mỗi chu kỳ")]
    [SerializeField] private float healAmount = 1f;
    [Tooltip("Chu kỳ hồi phục máu (giây)")]
    [SerializeField] private float healInterval = 1.25f;

    private float timeSinceLastDamage = 0f;
    private float healTimer = 0f;

    [Header("Gold Generation")]
    [Tooltip("Tốc độ sinh vàng ban đầu (vàng/giây)")]
    [SerializeField] private float baseGoldRate = 2f;
    [Tooltip("Thời gian để tăng tốc độ sinh vàng (giây)")]
    [SerializeField] private float goldScalingInterval = 30f;
    [Tooltip("Lượng vàng sinh thêm tăng thêm sau mỗi khoảng thời gian")]
    [SerializeField] private float goldScalingAmount = 1f;

    private float goldTimer = 0f;
    private float aliveTime = 0f;

    protected override void Start()
    {
        base.Start();
        // Cố định Base không cho di chuyển
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        timeSinceLastDamage = healDelay; // Cho phép hồi phục ngay nếu mất máu từ đầu (nếu có)
    }

    public override void TakeDamage(float damage)
    {
        if (dead) return;
        base.TakeDamage(damage);
        timeSinceLastDamage = 0f; // Nhận sát thương -> reset bộ đếm thời gian an toàn
    }

    private void Update()
    {
        if (dead) return;

        aliveTime += Time.deltaTime;

        // 1. Cơ chế sinh vàng tăng tiến theo thời gian cho Enemy (Cộng vào EnemyAI)
        goldTimer += Time.deltaTime;
        if (goldTimer >= 1f)
        {
            goldTimer -= 1f;
            int currentRate = Mathf.FloorToInt(baseGoldRate + (aliveTime / goldScalingInterval) * goldScalingAmount);
            if (EnemyAI.Instance != null)
            {
                EnemyAI.Instance.AddEnemyGold(currentRate);
            }
        }

        // 2. Cơ chế tự động hồi phục máu ngoài giao tranh (Out-of-Combat Auto-Healing)
        timeSinceLastDamage += Time.deltaTime;
        if (currentHealth < maxHealth && timeSinceLastDamage >= healDelay)
        {
            healTimer += Time.deltaTime;
            if (healTimer >= healInterval)
            {
                healTimer -= healInterval;
                TakeHealth(healAmount);
                if (currentHealth > maxHealth) currentHealth = maxHealth;
                Debug.Log($"EnemyBase ngoài giao tranh hồi phục: +{healAmount} HP. Máu hiện tại: {currentHealth}/{maxHealth}");
            }
        }
        else
        {
            healTimer = 0f;
        }
    }

    public override void EntityDie()
    {
        if (dead) return;
        base.EntityDie();
        
        // Khi Enemy Base bị phá hủy -> Player THẮNG
        if (GameResult.Instance != null)
        {
            GameResult.Instance.TriggerVictory();
        }
    }
}
