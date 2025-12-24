using UnityEngine;
using System.Collections.Generic;

public class UnitUpgrader : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private List<UnitLevelSO> unitLevels;

    private int _currentLevelIndex = 0;
    private Entity _entity;
    private EntityAttack _entityAttack;
    private Transform _visualContainer;

    private void Start()
    {
        _entity = GetComponent<Entity>();
        _entityAttack = GetComponent<EntityAttack>();
        _visualContainer = transform.Find("VisualContainer");
        // Giả sử bạn gom toàn bộ phần hình ảnh HeroEditor vào 1 Gameobject con tên "Visual"
        _visualContainer = transform.Find("Visual");

        if (unitLevels.Count > 0)
        {
            ApplyLevelStats(unitLevels[0]);
        }
    }

    public void TryUpgrade()
    {
        // 1. Kiểm tra max level
        if (_currentLevelIndex >= unitLevels.Count - 1)
        {
            Debug.Log("Max Level Reached!");
            return;
        }

        // 2. Lấy data cấp tiếp theo
        UnitLevelSO nextLevel = unitLevels[_currentLevelIndex + 1];

        // 3. Kiểm tra tiền
        if (CurrencyManager.Instance.TrySpendGold(nextLevel.upgradeCost))
        {
            _currentLevelIndex++;
            ApplyLevelStats(nextLevel);
            Debug.Log($"Upgraded to {nextLevel.levelName}");
        }
    }

    private void ApplyLevelStats(UnitLevelSO data)
    {
        // 1. Cập nhật Máu (Gọi hàm public của Entity)
        if (_entity != null)
        {
            _entity.SetMaxHealth(data.maxHealth);
            _entity.HealFull(); // Nâng cấp xong thì hồi đầy máu (Logic game thường thấy)
        }

        // 2. Cập nhật Damage
        if (_entityAttack != null)
        {
            _entityAttack.damage = data.damage;
        }

        // 3. Cập nhật Hình ảnh (Visual)
        if (data.visualPrefab != null && _visualContainer != null)
        {
            UpdateVisual(data.visualPrefab);
        }
    }

    private void UpdateVisual(GameObject newVisualPrefab)
    {
        // Xóa visual cũ
        foreach (Transform child in _visualContainer)
        {
            Destroy(child.gameObject);
        }

        // Tạo visual mới
        GameObject newVisual = Instantiate(newVisualPrefab, _visualContainer);

        // Reset lại vị trí/góc xoay nếu cần
        newVisual.transform.localPosition = Vector3.zero;
        newVisual.transform.localRotation = Quaternion.identity;

        // LƯU Ý QUAN TRỌNG:
        // Vì Animator nằm trên Visual mới, ta cần gán lại Animator cho các script Attack/Movement
        // Đây là bước Map lại tham chiếu (Dependency Injection thủ công)
        Animator newAnim = newVisual.GetComponent<Animator>();

        // Cần ép kiểu về SwordAttack nếu muốn gán Animator (hoặc sửa class cha để hỗ trợ)
        // Cách tốt nhất theo OOP: Dùng Interface hoặc method SetAnimator
        if (_entityAttack is SwordAttack swordAttack)
        {
            swordAttack.animator = newAnim;
        }

        // Tương tự với Movement nếu cần thiết
    }
}
