using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.EquipmentCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.EquipmentHandlers
{
    public class CmdRemoveEquipmentHandler : ICommandHandler<CmdRemoveEquipment>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveEquipmentHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public bool Handle(CmdRemoveEquipment command)
        {
            var equipments = _gameState.Equipments;
            var removedEquipment =
                equipments.FirstOrDefault(equipment => equipment.OwnerId == command.OwnerId);

            if (removedEquipment == null)
            {
                Debug.Log($"Couldn't find Inventory for ID: {command.OwnerId}");
                return false;
            }

            equipments.Remove(removedEquipment);
            return true;
        }
    }
}