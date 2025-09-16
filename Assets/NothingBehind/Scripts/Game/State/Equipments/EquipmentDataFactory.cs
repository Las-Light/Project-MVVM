using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State.Entities;

namespace NothingBehind.Scripts.Game.State.Equipments
{
    public static class EquipmentDataFactory
    {
        public static EquipmentData CreateEquipmentData(GameSettings gameSettings,
            EntityType entityType,
            int ownerId)
        {
            var equipmentSettings =
                gameSettings.EquipmentsSettings.AllEquipments.First(settings => settings.EntityType == entityType);
            var equipmentSlots = new List<EquipmentSlotData>();
            // foreach (var settingsSlot in equipmentSettings.Slots)
            // {
            //     var slot = new EquipmentSlotData();
            //     slot.SlotType = settingsSlot.SlotType;
            //     slot.ItemType = settingsSlot.ItemType;
            //     slot.Width = settingsSlot.Width;
            //     slot.Height = settingsSlot.Height;
            //     equipmentSlots.Add(slot);
            // }

            var equipment = new EquipmentData
            {
                OwnerId = ownerId,
                Slots = equipmentSlots,
                Width = equipmentSettings.Width,
                Height = equipmentSettings.Height
            };
            return equipment;
        }
    }
}