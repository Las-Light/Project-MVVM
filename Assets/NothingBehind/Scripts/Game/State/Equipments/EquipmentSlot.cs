using NothingBehind.Scripts.Game.State.Items;
using R3;

namespace NothingBehind.Scripts.Game.State.Equipments
{
    public class EquipmentSlot
    {
        public SlotType SlotType { get; }
        public ItemType ItemType { get; }
        public int Width { get; }
        public int Height { get; }
        public ReactiveProperty<Item?> EquippedItem { get; }

        public EquipmentSlot(EquipmentSlotData equipmentSlotData)
        {
            SlotType = equipmentSlotData.SlotType;
            ItemType = equipmentSlotData.ItemType;
            Width = equipmentSlotData.Width;
            Height = equipmentSlotData.Height;

            EquippedItem = new ReactiveProperty<Item?>(ItemsFactory.CreateItem(equipmentSlotData.EquippedItem));

            EquippedItem.Subscribe(value =>
            {
                equipmentSlotData.EquippedItem = value?.Origin;
            });
        }

        public void Equip(Item item)
        {
            EquippedItem.Value = item;
        }

        public bool Unequip()
        {
            if (EquippedItem != null)
            {
                EquippedItem.Value = null;
                return true;
            }

            return false;
        }
    }
}