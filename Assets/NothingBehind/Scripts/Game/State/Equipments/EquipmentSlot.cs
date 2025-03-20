using NothingBehind.Scripts.Game.State.Items;
using R3;

namespace NothingBehind.Scripts.Game.State.Equipments
{
    public class EquipmentSlot
    {
        public SlotType SlotType { get; }
        public ItemType ItemType { get; }
        public ReactiveProperty<Item> EquippedItem { get; }

        public EquipmentSlot(EquipmentSlotData equipmentSlotData)
        {
            SlotType = equipmentSlotData.SlotType;
            ItemType = equipmentSlotData.ItemType;

            EquippedItem = new ReactiveProperty<Item>(ItemsFactory.CreateItem(equipmentSlotData.EquippedItem));

            EquippedItem.Subscribe(value => equipmentSlotData.EquippedItem = value.Origin);
        }

        public bool TryEquip(Item item)
        {
            if (item.ItemType == ItemType)
            {
                EquippedItem.Value = item;
                return true;
            }

            return false;
        }
        
        public void Unequip()
        {
            EquippedItem.Value = null;
        }
    }
}