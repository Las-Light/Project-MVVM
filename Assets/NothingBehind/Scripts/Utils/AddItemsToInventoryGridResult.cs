namespace NothingBehind.Scripts.Utils
{
    public readonly struct AddItemsToInventoryGridResult
    {
        public readonly string ItemTypeId;
        public readonly int ItemsToAddAmount;
        public readonly int ItemsAddedAmount;
        public readonly bool Success;

        public int ItemsNotAddedAmount => ItemsToAddAmount - ItemsAddedAmount;

        public AddItemsToInventoryGridResult(string itemTypeId, int itemsToAddAmount, int itemsAddedAmount, bool success)
        {
            ItemTypeId = itemTypeId;
            ItemsToAddAmount = itemsToAddAmount;
            ItemsAddedAmount = itemsAddedAmount;
            Success = success;
        }
    }
}