using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.CharactersCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.CharactersHandlers
{
    public class CmdRemoveCharacterHandler : ICommandHandler<CmdRemoveCharacter>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveCharacterHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public CommandResult Handle(CmdRemoveCharacter command)
        {
            var currentMap = _gameState.Maps.FirstOrDefault(m => m.Id == _gameState.CurrentMapId.CurrentValue);
            if (currentMap == null)
            {
                Debug.Log($"Couldn't find Mapstate for ID: {_gameState.CurrentMapId.CurrentValue}");
                return new CommandResult(false);
            }

            var removedCharacter = currentMap.Characters.FirstOrDefault(character => character.Id == command.Id);
            if (removedCharacter == null)
            {
                Debug.Log($"Couldn't find Character for ID: {command.Id}");
                return new CommandResult(command.Id, false);
            }

            currentMap.Characters.Remove(removedCharacter);
            command.InventoryService.RemoveInventory(command.Id);
            command.EquipmentService.RemoveEquipment(command.Id);
            command.ArsenalService.RemoveArsenal(command.Id);
            return new CommandResult(command.Id, true);
        }
    }
}