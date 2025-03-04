using NothingBehind.Scripts.Game.State.Inventory;
using UnityEngine;

namespace NothingBehind.Scripts.Utils
{
    public readonly struct AddItemsToInventoryGridResult
    {
        public readonly ItemType ItemType;
        public readonly int ItemId;
        public readonly int ItemsToAddAmount;
        public readonly int ItemsAddedAmount;
        public readonly bool NeedRemove;
        public readonly bool Success;

        public int ItemsNotAddedAmount => ItemsToAddAmount - ItemsAddedAmount;

        public AddItemsToInventoryGridResult(ItemType itemType, int itemId, int itemsToAddAmount,
            int itemsAddedAmount, bool needRemove, bool success)
        {
            ItemType = itemType;
            ItemId = itemId;
            ItemsToAddAmount = itemsToAddAmount;
            ItemsAddedAmount = itemsAddedAmount;
            NeedRemove = needRemove;
            Success = success;
        }
    }
}