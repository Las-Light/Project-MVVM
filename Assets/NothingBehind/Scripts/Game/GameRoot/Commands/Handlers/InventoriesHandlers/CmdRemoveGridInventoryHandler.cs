using System.Linq;
using NothingBehind.Scripts.Game.GameRoot.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.InventoriesHandlers
{
    public class CmdRemoveGridInventoryHandler : ICommandHandler<CmdRemoveGridFromInventory>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveGridInventoryHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public CommandResult Handle(CmdRemoveGridFromInventory command)
        {
            var inventory = _gameState.Inventories.First(inventory => inventory.OwnerId == command.OwnerId);
            var removedInventoryGrid = inventory.InventoryGrids.FirstOrDefault(grid => grid.GridId == command.Grid.GridId);

            if (removedInventoryGrid == null)
            {
                Debug.LogError($"Couldn't find InventoryGrid with ID: {command.Grid} for owner with ID: {command.OwnerId}");
                return new CommandResult(command.Grid.GridId, false);
            }
            
            inventory.InventoryGrids.Remove(removedInventoryGrid);
            return new CommandResult(command.Grid.GridId, true);
        }
    }
}