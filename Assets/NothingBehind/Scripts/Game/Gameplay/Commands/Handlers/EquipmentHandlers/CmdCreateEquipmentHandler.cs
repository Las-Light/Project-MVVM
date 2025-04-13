using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.EquipmentCommands;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Equipment;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.EquipmentHandlers
{
    public class CmdCreateEquipmentHandler : ICommandHandler<CmdCreateEquipment>
    {
        private readonly GameStateProxy _gameState;
        private readonly EquipmentsSettings _equipmentsSettings;
        private readonly GameSettings _gameSettings;

        public CmdCreateEquipmentHandler(GameStateProxy gameState,
            EquipmentsSettings equipmentsSettings,
            GameSettings gameSettings)
        {
            _gameState = gameState;
            _equipmentsSettings = equipmentsSettings;
            _gameSettings = gameSettings;
        }

        public CommandResult Handle(CmdCreateEquipment command)
        {
            var equipmentSettings = _equipmentsSettings.AllEquipments
                .First(settings => settings.EntityType == command.OwnerType);
            var equipmentSlots = new List<EquipmentSlotData>();
            foreach (var settingsSlot in equipmentSettings.Slots)
            {
                var slot = new EquipmentSlotData();
                slot.SlotType = settingsSlot.SlotType;
                slot.ItemType = settingsSlot.ItemType;
                if (settingsSlot.EquippedItemSettings != null)
                {
                    slot.EquippedItem = ItemsDataFactory.CreateItemData(_gameState.GameState, 
                        _gameSettings, settingsSlot.EquippedItemSettings);
                }
                equipmentSlots.Add(slot);
            }

            var equipment = new EquipmentData
            {
                OwnerId = command.OwnerId,
                Slots = equipmentSlots
            };

            _gameState.Equipments.Add(new Equipment(equipment));

            return new CommandResult(command.OwnerId,true);
        }
    }
}