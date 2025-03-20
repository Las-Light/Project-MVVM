using System;
using NothingBehind.Scripts.Game.State.Items;

namespace NothingBehind.Scripts.Game.State.Equipments
{
    [Serializable]
    public class EquipmentSlotData
    {
        public SlotType SlotType;
        public ItemType ItemType;
        public ItemData EquippedItem;
    }
}