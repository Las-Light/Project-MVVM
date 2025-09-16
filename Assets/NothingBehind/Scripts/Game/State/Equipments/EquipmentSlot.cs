using NothingBehind.Scripts.Game.State.Items;
using R3;
using UnityEngine;

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
            Debug.Log("Constr EqSlot");
            SlotType = equipmentSlotData.SlotType;
            ItemType = equipmentSlotData.ItemType;
            Width = equipmentSlotData.Width;
            Height = equipmentSlotData.Height;

            EquippedItem = new ReactiveProperty<Item?>(ItemsFactory.CreateItem(equipmentSlotData.EquippedItem));
            if (EquippedItem.Value != null)
            {
                Debug.Log(EquippedItem.Value.ItemType);
            }

            EquippedItem.Subscribe(value =>
            {
                equipmentSlotData.EquippedItem = value?.Origin;
            });
        }

        public void Equip(Item item)
        {
            Debug.Log("Equip" + item);
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