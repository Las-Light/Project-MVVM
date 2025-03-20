using System;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using ObservableCollections;

namespace NothingBehind.Scripts.Game.Gameplay.View.Equipments
{
    public class EquipmentViewModel : IDisposable
    {
        private readonly Equipment _equipment;

        public int OwnerId { get; }
        
        public IReadOnlyObservableDictionary<int, Item> AllEquippedItems => _equippedItemsMap;
        public IReadOnlyObservableDictionary<SlotType, EquipmentSlot> SlotsMap => _slotsMap;

        private readonly ObservableDictionary<int, Item> _equippedItemsMap = new();
        private readonly ObservableDictionary<SlotType, EquipmentSlot> _slotsMap = new();


        public EquipmentViewModel(Equipment equipment, EquipmentService equipmentService)
        {
            _equipment = equipment;
            OwnerId = equipment.OwnerId;

            foreach (var slot in equipment.Slots)
            {
                _equippedItemsMap[slot.EquippedItem.Value.Id] = slot.EquippedItem.Value;
                _slotsMap[slot.SlotType] = slot;
            }
        }

        public void EquipItem(SlotType slotType, Item item)
        {
            if (_slotsMap.TryGetValue(slotType, out var slot))
            {
                if (slot.TryEquip(item))
                {
                    _equippedItemsMap[item.Id] = item;
                }
            }
        }

        public void UnequipItem(SlotType slotType)
        {
            if (_slotsMap.TryGetValue(slotType, out var slot))
            {
                _equippedItemsMap.Remove(slot.EquippedItem.Value.Id);
                slot.Unequip();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}