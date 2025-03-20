using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Equipment
{
    [CreateAssetMenu(fileName = "Equipment Slot Config", menuName = "Equipment/Equipment Slot Config", order = 1)]

    public class EquipmentSlotSettings : ScriptableObject
    {
        public SlotType SlotType;
        public ItemType ItemType;
        public ItemSettings EquippedItemSettings;
    }
}