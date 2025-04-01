using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.StoragesCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.StoragesHandlers
{
    public class CmdRemoveStorageHandler : ICommandHandler<CmdRemoveStorage>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveStorageHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public CommandResult Handle(CmdRemoveStorage command)
        {
            var currentMap = _gameState.Maps.FirstOrDefault(m => m.Id == _gameState.CurrentMapId.CurrentValue);
            if (currentMap == null)
            {
                Debug.Log($"Couldn't find Mapstate for ID: {_gameState.CurrentMapId.CurrentValue}");
                return new CommandResult(false);
            }

            var removedStorage = currentMap.Storages.FirstOrDefault(storage => storage.Id == command.Id);
            if (removedStorage == null)
            {
                Debug.Log($"Couldn't find Storage for ID: {command.Id}");
                return new CommandResult(command.Id, false);;
            }

            currentMap.Storages.Remove(removedStorage);
            command.InventoryService.RemoveInventory(command.Id);
            return new CommandResult(command.Id, true);
        }
    }
}