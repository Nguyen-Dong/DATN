/// <summary>
/// Dịch vụ Cửa hàng (meta): mở khóa loại lính bằng KIM CƯƠNG.
/// Dùng cho UI Cửa hàng — gọi <see cref="TryUnlock"/> khi người chơi bấm mua chiêu.
/// </summary>
public static class TroopShop
{
    public enum Result { Success, AlreadyUnlocked, NotEnoughDiamonds, Invalid }

    public static Result TryUnlock(UnitDefinition def)
    {
        if (def == null) return Result.Invalid;
        if (TroopUnlockStore.IsUnlocked(def)) return Result.AlreadyUnlocked;
        if (CurrencyManager.Instance == null) return Result.Invalid;

        if (!CurrencyManager.Instance.TrySpendDiamonds(def.diamondUnlockCost))
            return Result.NotEnoughDiamonds;

        TroopUnlockStore.Unlock(def);
        return Result.Success;
    }
}
