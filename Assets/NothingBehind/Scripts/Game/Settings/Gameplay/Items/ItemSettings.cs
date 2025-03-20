using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Items
{
    [CreateAssetMenu(fileName = "Item Config", menuName = "Items/Item Config", order = 0)]
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

        [Space(5)]
        [Header("InventoryGridItem")] 
        public InventoryGridType GridType;
        public InventoryGridSettings GridSettings;
    }
}