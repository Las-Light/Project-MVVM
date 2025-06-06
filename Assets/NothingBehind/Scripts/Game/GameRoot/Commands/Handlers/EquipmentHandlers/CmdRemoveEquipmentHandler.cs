using System.Linq;
using NothingBehind.Scripts.Game.GameRoot.Commands.EquipmentCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.EquipmentHandlers
{
    public class CmdRemoveEquipmentHandler : ICommandHandler<CmdRemoveEquipment>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveEquipmentHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public CommandResult Handle(CmdRemoveEquipment command)
        {
            var equipments = _gameState.Equipments;
            var removedEquipment =
                equipments.FirstOrDefault(equipment => equipment.OwnerId == command.OwnerId);

            if (removedEquipment == null)
            {
                Debug.Log($"Couldn't find Inventory for ID: {command.OwnerId}");
                return new CommandResult(false);
            }

            equipments.Remove(removedEquipment);
            return new CommandResult(command.OwnerId,true);
        }
    }
}