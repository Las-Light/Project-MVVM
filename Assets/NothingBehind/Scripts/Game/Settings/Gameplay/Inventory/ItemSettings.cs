using NothingBehind.Scripts.Game.State.Inventory;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Item Config", menuName = "Inventory/Item Config", order = 3)]
    public class ItemSettings : ScriptableObject
    {
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