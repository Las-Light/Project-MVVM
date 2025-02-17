using System;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    [Serializable]
    public class ItemData
    {
        public string Id;
        public int Width;
        public int Height;
        public bool CanRotate;
        public bool IsRotated;
    }
}