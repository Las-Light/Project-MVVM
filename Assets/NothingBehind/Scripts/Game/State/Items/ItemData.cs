using System;

namespace NothingBehind.Scripts.Game.State.Items
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
    }
}