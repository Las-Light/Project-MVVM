using System;
using System.Linq;
using JetBrains.Annotations;
using NothingBehind.Scripts.Game.Gameplay.Commands.EquipmentCommands;
using NothingBehind.Scripts.Game.Gameplay.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.AmmoItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.ArmorItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.MagazinesItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.WeaponItems;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments
{
    public class EquipmentViewModel : IDisposable
    {
        private readonly Equipment _equipment;
        private readonly ItemsSettings _itemsSettings;

        public int OwnerId { get; }
        public int Width { get; }
        public int Height { get; }

        public IReadOnlyObservableDictionary<SlotType, Item> AllEquippedItems => _equippedItemsMap;
        public IReadOnlyObservableDictionary<int, ItemViewModel> ItemViewModelsMap => _itemViewModelsMap;
        public IReadOnlyObservableDictionary<SlotType, EquipmentSlot> SlotsMap => _slotsMap;
        public IReadOnlyObservableList<EquipmentSlot> Slots;

        private readonly ObservableDictionary<SlotType, Item> _equippedItemsMap = new();
        private readonly ObservableDictionary<int, EquipmentSlot> _itemSlotsMap = new();
        private readonly ObservableDictionary<SlotType, EquipmentSlot> _slotsMap = new();
        private readonly ObservableDictionary<int, ItemViewModel> _itemViewModelsMap = new();
        private ICommandProcessor _commandProcessor;


        public EquipmentViewModel(Equipment equipment, 
            ItemsSettings itemsSettings, 
            EquipmentService equipmentService,
            ICommandProcessor commandProcessor)
        {
            _equipment = equipment;
            _itemsSettings = itemsSettings;
            OwnerId = equipment.OwnerId;
            Width = equipment.Width;
            Height = equipment.Height;
            Slots = equipment.Slots;
            _commandProcessor = commandProcessor;

            foreach (var slot in equipment.Slots)
            {
                if (slot.EquippedItem.Value != null)
                {
                    CreateItemViewModel(slot.EquippedItem.Value, itemsSettings);
                    _equippedItemsMap[slot.SlotType] = slot.EquippedItem.Value;
                    _itemSlotsMap[slot.EquippedItem.Value.Id] = slot;
                }

                _slotsMap[slot.SlotType] = slot;
            }
        }

        public bool TryEquipItem(SlotType slotType, Item item)
        {
            if (_slotsMap.TryGetValue(slotType, out var slot))
            {
                if (slot.EquippedItem.Value != item)
                {
                    if (CanEquipItem(slotType, item))
                    {
                        if (_itemSlotsMap.TryGetValue(item.Id, out var oldSlot))
                        {
                            TryUnequipItem(oldSlot.SlotType);
                        }
                        CreateItemViewModel(item, _itemsSettings);
                        //slot.Equip(item);
                        EquipItem(OwnerId, slot, item);
                        _equippedItemsMap[slotType] = item;
                        _itemSlotsMap[item.Id] = slot;
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        public bool TryUnequipItem(SlotType slotType)
        {
            if (_slotsMap.TryGetValue(slotType, out var slot))
            {
                RemoveItemViewModel(slot.EquippedItem.Value);
                _equippedItemsMap.Remove(slotType);
                _itemSlotsMap.Remove(slot.EquippedItem.Value.Id);
                //slot.Unequip();
                UnequipItem(OwnerId, slot);
                return true;
            }

            return false;
        }

        [CanBeNull]
        public Item GetItemAtSlot(SlotType slotType)
        {
            if (_equippedItemsMap.TryGetValue(slotType, out var item))
            {
                switch (item)
                {
                    case GridItem gridItem:
                        return gridItem;
                    case WeaponItem weaponItem:
                        return weaponItem;
                    case ArmorItem armorItem:
                        return armorItem;
                }
            }

            return null;
        }

        public bool CanEquipItem(SlotType slotType, Item item)
        {
            if (_slotsMap.TryGetValue(slotType, out var slot))
            {
                return slot.ItemType == item.ItemType;
            }

            return false;
        }

        public bool IsEmptyEquipmentSlots()
        {
            if (_itemSlotsMap.Count > 0)
            {
                return false;
            }

            return true;
        }
        
        // Добавляет InventorGrid в Inventory
        public CommandResult EquipItem(int ownerId, EquipmentSlot slot, Item item)
        {
            var command = new CmdEquipItem(ownerId, slot, item);
            var result = _commandProcessor.Process(command);

            return result;
        }

        // Удаляет InventorGrid из Inventory
        public CommandResult UnequipItem(int ownerId, EquipmentSlot slot)
        {
            var command = new CmdUnequipItem(ownerId, slot);
            var result = _commandProcessor.Process(command);

            return result;
        }

        private void CreateItemViewModel(Item item, ItemsSettings itemsSettings)
        {
            var itemSettings = itemsSettings.Items.FirstOrDefault(itemConfig =>
            {
                switch (item)
                {
                    case AmmoItem ammoItem:
                        return itemConfig.Caliber == ammoItem.Caliber;
                    case GridItem gridItem:
                        return itemConfig.GridType == gridItem.GridType;
                    case MagazinesItem magazinesItem:
                        return itemConfig.MagazinesCaliber == magazinesItem.Magazines.Caliber;
                    case WeaponItem weaponItem:
                        return itemConfig.WeaponName == weaponItem.Weapon.WeaponName;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(item));
                }
            });
            if (itemSettings == null)
            {
                Debug.LogError($"ItemSettings with type {item.ItemType} not found");
            }

            var itemViewModel = new ItemViewModel(item, itemSettings);
            _itemViewModelsMap[item.Id] = itemViewModel;
        }

        private void RemoveItemViewModel(Item item)
        {
            if (_itemViewModelsMap.TryGetValue(item.Id, out _))
            {
                _itemViewModelsMap.Remove(item.Id);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}