using System;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.AmmoItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.ArmorItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.MagazinesItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.WeaponItems;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Items
{
    public static class ItemsFactory
    {
        public static Item CreateItem(ItemData itemData)
        {
            if (itemData != null)
            {
                switch (itemData.ItemType)
                {
                    case ItemType.Backpack or ItemType.ChestRig:
                        return new GridItem(itemData as GridItemData);

                    case ItemType.Armor:
                        return new ArmorItem(itemData as ArmorItemData);

                    case ItemType.Weapon:
                        return new WeaponItem(itemData as WeaponItemData);
                    
                    case ItemType.Ammo:
                        return new AmmoItem(itemData as AmmoItemData);
                    
                    case ItemType.Magazines:
                        return new MagazinesItem(itemData as MagazinesItemData);

                    default:
                        throw new Exception("Unsupported item type: " + itemData.ItemType);
                }
            }

            return null;
        }
    }
}