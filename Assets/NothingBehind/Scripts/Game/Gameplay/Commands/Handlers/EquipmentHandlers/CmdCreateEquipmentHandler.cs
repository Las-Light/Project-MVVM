using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.EquipmentCommands;
using NothingBehind.Scripts.Game.Settings.Equipment;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.EquipmentHandlers
{
    public class CmdCreateEquipmentHandler : ICommandHandler<CmdCreateEquipment>
    {
        private readonly GameStateProxy _gameState;
        private readonly EquipmentsSettings _equipmentsSettings;

        public CmdCreateEquipmentHandler(GameStateProxy gameState,
            EquipmentsSettings equipmentsSettings)
        {
            _gameState = gameState;
            _equipmentsSettings = equipmentsSettings;
        }

        public bool Handle(CmdCreateEquipment command)
        {
            var equipmentSettings = _equipmentsSettings.AllEquipments
                .First(settings => settings.EntityType == command.OwnerType);
            var equipmentSlots = new List<EquipmentSlotData>();
            foreach (var settingsSlot in equipmentSettings.Slots)
            {
                var itemSettings = settingsSlot.EquippedItemSettings;
                var slot = new EquipmentSlotData()
                {
                    SlotType = settingsSlot.SlotType,
                    ItemType = settingsSlot.ItemType,
                    EquippedItem = ItemsDataFactory.CreateItemData(_gameState._gameState, itemSettings)
                };
                equipmentSlots.Add(slot);
            }

            var equipment = new EquipmentData
            {
                OwnerId = command.OwnerId,
                Slots = equipmentSlots
            };

            _gameState.Equipments.Add(new State.Equipments.Equipment(equipment));

            return true;
        }
    }
}