namespace NothingBehind.Scripts.Utils
{
    public readonly struct RemoveItemsFromInventoryGridResult
    {
        public readonly string ItemTypeId;
        public readonly int ItemsToRemoveAmount;
        public readonly bool Success;

        public RemoveItemsFromInventoryGridResult(string itemTypeId, int itemsToRemoveAmount, bool success)
        {
            ItemTypeId = itemTypeId;
            ItemsToRemoveAmount = itemsToRemoveAmount;
            Success = success;
        }
    }
}