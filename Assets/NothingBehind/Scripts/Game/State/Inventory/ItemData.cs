using System;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    [Serializable]
    public class ItemData
    {
        public string Id;
        public string ItemType;
        public int Width;
        public int Height;
        public bool CanRotate;
        public bool IsRotated;
        public bool IsStackable;
        public int MaxStackSize;
        public int CurrentStack;
    }
}