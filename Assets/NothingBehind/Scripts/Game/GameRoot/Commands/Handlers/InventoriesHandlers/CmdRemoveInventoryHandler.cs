using System.Linq;
using NothingBehind.Scripts.Game.GameRoot.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.InventoriesHandlers
{
    public class CmdRemoveInventoryHandler : ICommandHandler<CmdRemoveInventory>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveInventoryHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public CommandResult Handle(CmdRemoveInventory command)
        {
            var inventories = _gameState.Inventories;
            var removedInventory =
                inventories.FirstOrDefault(inventory => inventory.OwnerId == command.OwnerId);

            if (removedInventory == null)
            {
                Debug.Log($"Couldn't find Inventory for ID: {command.OwnerId}");
                return new CommandResult(command.OwnerId, false);;
            }

            inventories.Remove(removedInventory);
            return new CommandResult(command.OwnerId, true);
        }
    }
}