using NothingBehind.Scripts.Game.State.Items;
using UnityEngine;

namespace NothingBehind.Scripts.Utils
{
    public readonly struct AddItemAmountResult
    {
        public readonly ItemType ItemType;
        public readonly int ItemId;
        public readonly int ItemsToAddAmount;
        public readonly int ItemsAddedAmount;
        public readonly bool NeedRemove;
        public readonly bool Success;

        public int ItemsNotAddedAmount => ItemsToAddAmount - ItemsAddedAmount;

        public AddItemAmountResult(ItemType itemType, int itemId, int itemsToAddAmount,
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