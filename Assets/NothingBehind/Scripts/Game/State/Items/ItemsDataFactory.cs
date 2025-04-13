using System;
using System.Linq;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.AmmoItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.ArmorItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.MagazinesItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.WeaponItems;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Game.State.Weapons;

namespace NothingBehind.Scripts.Game.State.Items
{
    public static class ItemsDataFactory
    {
        public static ItemData CreateItemData(GameState gameState,
            GameSettings gameSettings,
            ItemSettings itemSettings)
        {
            switch (itemSettings.ItemType)
            {
                case ItemType.Backpack or ItemType.ChestRig:
                    var gridItemData = new GridItemData();
                    gridItemData.Id = gameState.CreateItemId();
                    gridItemData.ItemType = itemSettings.ItemType;
                    gridItemData.Width = itemSettings.Width;
                    gridItemData.Height = itemSettings.Height;
                    gridItemData.Weight = itemSettings.Weight;
                    gridItemData.CanRotate = itemSettings.CanRotate;
                    gridItemData.IsRotated = itemSettings.IsRotated;
                    gridItemData.IsStackable = itemSettings.IsStackable;
                    gridItemData.MaxStackSize = itemSettings.MaxStackSize;
                    gridItemData.CurrentStack = itemSettings.CurrentStack;
                    gridItemData.GridType = itemSettings.GridType;
                    gridItemData.GridData =
                        InventoryGridsDataFactory.CreateInventorGridData(gameState, itemSettings.GridSettings);
                    gridItemData.GridId = gridItemData.GridData.GridId;
                    return gridItemData;

                case ItemType.Armor:
                    var armorItemData = new ArmorItemData();
                    armorItemData.Id = gameState.CreateItemId();
                    armorItemData.ItemType = itemSettings.ItemType;
                    armorItemData.Width = itemSettings.Width;
                    armorItemData.Height = itemSettings.Height;
                    armorItemData.Weight = itemSettings.Weight;
                    armorItemData.CanRotate = itemSettings.CanRotate;
                    armorItemData.IsRotated = itemSettings.IsRotated;
                    armorItemData.IsStackable = itemSettings.IsStackable;
                    armorItemData.MaxStackSize = itemSettings.MaxStackSize;
                    armorItemData.CurrentStack = itemSettings.CurrentStack;
                    return armorItemData;

                case ItemType.Weapon:
                    var weaponSettings =
                        gameSettings.WeaponsSettings.WeaponConfigs.First(config =>
                            config.WeaponName == itemSettings.WeaponName);
                    var weaponItemData = new WeaponItemData();
                    weaponItemData.Id = gameState.CreateItemId();
                    weaponItemData.ItemType = itemSettings.ItemType;
                    weaponItemData.Width = itemSettings.Width;
                    weaponItemData.Height = itemSettings.Height;
                    weaponItemData.Weight = itemSettings.Weight;
                    weaponItemData.CanRotate = itemSettings.CanRotate;
                    weaponItemData.IsRotated = itemSettings.IsRotated;
                    weaponItemData.IsStackable = itemSettings.IsStackable;
                    weaponItemData.MaxStackSize = itemSettings.MaxStackSize;
                    weaponItemData.CurrentStack = itemSettings.CurrentStack;
                    weaponItemData.WeaponData = WeaponDataFactory.CreateWeaponData(gameState, gameSettings,
                        weaponItemData.Id, weaponSettings.WeaponName);
                    return weaponItemData;
                
                case ItemType.Ammo:
                    var ammoItemData = new AmmoItemData();
                    ammoItemData.Id = gameState.CreateItemId();
                    ammoItemData.ItemType = itemSettings.ItemType;
                    ammoItemData.Width = itemSettings.Width;
                    ammoItemData.Height = itemSettings.Height;
                    ammoItemData.Weight = itemSettings.Weight;
                    ammoItemData.CanRotate = itemSettings.CanRotate;
                    ammoItemData.IsRotated = itemSettings.IsRotated;
                    ammoItemData.IsStackable = itemSettings.IsStackable;
                    ammoItemData.MaxStackSize = itemSettings.MaxStackSize;
                    ammoItemData.CurrentStack = itemSettings.CurrentStack;
                    ammoItemData.Caliber = itemSettings.Caliber;
                    return ammoItemData;
                
                case ItemType.Magazines:
                    var magazinesItemData = new MagazinesItemData();
                    magazinesItemData.Id = gameState.CreateItemId();
                    magazinesItemData.ItemType = itemSettings.ItemType;
                    magazinesItemData.Width = itemSettings.Width;
                    magazinesItemData.Height = itemSettings.Height;
                    magazinesItemData.Weight = itemSettings.Weight;
                    magazinesItemData.CanRotate = itemSettings.CanRotate;
                    magazinesItemData.IsRotated = itemSettings.IsRotated;
                    magazinesItemData.IsStackable = itemSettings.IsStackable;
                    magazinesItemData.MaxStackSize = itemSettings.MaxStackSize;
                    magazinesItemData.CurrentStack = itemSettings.CurrentStack;
                    magazinesItemData.Magazines = WeaponDataFactory.CreateMagazinesData(itemSettings.MagazinesSettings);
                    return magazinesItemData;

                default:
                    throw new Exception("Unsupported item type: " + itemSettings.ItemType);
            }
        }
    }
}