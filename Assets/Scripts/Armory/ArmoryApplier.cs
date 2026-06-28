using UnityEngine;

/// <summary>
/// Gắn lên prefab GỐC của lính (cùng chỗ Entity). Khi spawn: áp cấp Armory đã lưu lên lính
/// (đổi visual vũ khí/giáp + gán ATK/DEF). Logic chi tiết ở <see cref="ArmoryVisual"/>.
/// </summary>
public class ArmoryApplier : MonoBehaviour
{
    [Tooltip("Cấu hình nâng cấp của loại lính này")]
    [SerializeField] private TroopUpgradeSO upgradeData;

    private void Start()
    {
        if (upgradeData == null)
        {
            Debug.LogWarning($"ArmoryApplier ({name}): chưa gán TroopUpgradeSO.");
            return;
        }
        ArmoryVisual.Apply(gameObject, upgradeData, applyStats: true);
    }
}
