using System;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    [Serializable]
    public class ItemData
    {
        public int Id;
        public ItemType ItemType;
        public int Width;
        public int Height;
        public int Weight;
        public bool CanRotate;
        public bool IsRotated;
        public bool IsStackable;
        public int MaxStackSize;
        public int CurrentStack;

        public ItemData(int id, ItemType itemType, int width, int height, int weight, bool canRotate, bool isRotated, bool isStackable, int maxStackSize, int currentStack)
        {
            Id = id;
            ItemType = itemType;
            Width = width;
            Height = height;
            Weight = weight;
            CanRotate = canRotate;
            IsRotated = isRotated;
            IsStackable = isStackable;
            MaxStackSize = maxStackSize;
            CurrentStack = currentStack;
        }
    }
}