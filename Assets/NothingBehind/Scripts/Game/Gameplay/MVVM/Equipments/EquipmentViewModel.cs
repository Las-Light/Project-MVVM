using System;
using System.Linq;
using JetBrains.Annotations;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.ArmorItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.WeaponItems;
using ObservableCollections;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments
{
    public class EquipmentViewModel : IDisposable
    {
        private readonly Equipment _equipment;
        private readonly ItemsSettings _itemsSettings;

        public int OwnerId { get; }

        public IReadOnlyObservableDictionary<SlotType, Item> AllEquippedItems => _equippedItemsMap;
        public IReadOnlyObservableDictionary<SlotType, EquipmentSlot> SlotsMap => _slotsMap;

        private readonly ObservableDictionary<SlotType, Item> _equippedItemsMap = new();
        private readonly ObservableDictionary<SlotType, EquipmentSlot> _slotsMap = new();
        private readonly ObservableDictionary<int, ItemViewModel> _itemViewModelsMap = new();


        public EquipmentViewModel(Equipment equipment, ItemsSettings itemsSettings, EquipmentService equipmentService)
        {
            _equipment = equipment;
            _itemsSettings = itemsSettings;
            OwnerId = equipment.OwnerId;

            foreach (var slot in equipment.Slots)
            {
                if (slot.EquippedItem.Value != null)
                {
                    _equippedItemsMap[slot.SlotType] = slot.EquippedItem.Value;
                    CreateItemViewModel(slot.EquippedItem.Value, itemsSettings);
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
                        slot.Equip(item);
                        _equippedItemsMap[slotType] = item;
                        CreateItemViewModel(item, _itemsSettings);
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
                slot.Unequip();
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

        private void CreateItemViewModel(Item item, ItemsSettings itemsSettings)
        {
            var itemSettings = itemsSettings.Items.FirstOrDefault(itemConfig => itemConfig.ItemType == item.ItemType);
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