using System;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.ArmorItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.WeaponItems;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Items
{
    public static class ItemsDataFactory
    {
        public static ItemData CreateItemData(GameState gameState, ItemSettings itemSettings)
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
                    gridItemData.GridId = gameState.CreateGridId();
                    gridItemData.GridType = itemSettings.GridType;
                    gridItemData.GridData =
                        InventoryGridsDataFactory.CreateInventorGridData(gameState, itemSettings.GridSettings);
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
                    return weaponItemData;

                default:
                    throw new Exception("Unsupported item type: " + itemSettings.ItemType);
            }
        }
    }
}