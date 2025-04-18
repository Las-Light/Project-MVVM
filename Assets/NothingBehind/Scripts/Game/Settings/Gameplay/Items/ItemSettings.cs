using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Weapons;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
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
        public Color Color;

        [Space(5)]
        [Header("InventoryGridItem")] 
        public InventoryGridType GridType;
        public InventoryGridSettings GridSettings;

        [Space(5)] 
        [Header("WeaponItem")] 
        public WeaponType WeaponType;
        public WeaponName WeaponName;

        [Space(5)] [Header("AmmoItem")] 
        public string Caliber;

        [Space(5)] [Header("MagazinesItem")] 
        public string MagazinesCaliber;
        public MagazinesSettings MagazinesSettings;
    }
}