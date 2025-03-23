using System;
using JetBrains.Annotations;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.ArmorItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.WeaponItems;
using ObservableCollections;

namespace NothingBehind.Scripts.Game.Gameplay.View.Equipments
{
    public class EquipmentViewModel : IDisposable
    {
        private readonly Equipment _equipment;

        public int OwnerId { get; }
        
        public IReadOnlyObservableDictionary<SlotType, Item> AllEquippedItems => _equippedItemsMap;
        public IReadOnlyObservableDictionary<SlotType, EquipmentSlot> SlotsMap => _slotsMap;

        private readonly ObservableDictionary<SlotType, Item> _equippedItemsMap = new();
        private readonly ObservableDictionary<SlotType, EquipmentSlot> _slotsMap = new();


        public EquipmentViewModel(Equipment equipment, EquipmentService equipmentService)
        {
            _equipment = equipment;
            OwnerId = equipment.OwnerId;

            foreach (var slot in equipment.Slots)
            {
                _equippedItemsMap[slot.SlotType] = slot.EquippedItem.Value;
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}