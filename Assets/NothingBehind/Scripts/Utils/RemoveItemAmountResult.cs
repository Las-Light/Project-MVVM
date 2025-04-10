using NothingBehind.Scripts.Game.State.Items;

namespace NothingBehind.Scripts.Utils
{
    public readonly struct RemoveItemAmountResult
    {
        public readonly ItemType ItemTypeId;
        public readonly int ItemId;
        public readonly int ItemsToRemoveAmount;
        public readonly bool Success;

        public RemoveItemAmountResult(ItemType itemTypeId, int itemId, int itemsToRemoveAmount, bool success)
        {
            ItemTypeId = itemTypeId;
            ItemId = itemId;
            ItemsToRemoveAmount = itemsToRemoveAmount;
            Success = success;
        }
    }
}