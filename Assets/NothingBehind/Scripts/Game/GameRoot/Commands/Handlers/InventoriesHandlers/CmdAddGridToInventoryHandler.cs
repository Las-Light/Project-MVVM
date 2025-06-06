using System.Linq;
using NothingBehind.Scripts.Game.GameRoot.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.InventoriesHandlers
{
    public class CmdAddGridToInventoryHandler : ICommandHandler<CmdAddGridToInventory>
    {
        private readonly GameStateProxy _gameState;

        public CmdAddGridToInventoryHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public CommandResult Handle(CmdAddGridToInventory command)
        {
            var inventory = _gameState.Inventories.First(inventory => inventory.OwnerId == command.OwnerId);
            if (inventory.InventoryGrids.FirstOrDefault(grid => grid.GridId == command.Grid.GridId) != null)
            {
                Debug.LogError(
                    $"InventoryGrid with Id {command.Grid} already exists in Inventory {inventory.OwnerType}-{inventory.OwnerId}.");
                return new CommandResult(command.Grid.GridId,false);
            }

            if (command.Grid is InventoryGridWithSubGrid subGrid)
            {
                inventory.InventoryGrids.Add(subGrid);
            }
            else
            {
                inventory.InventoryGrids.Add(command.Grid);
            }

            return new CommandResult(command.Grid.GridId, true);
        }
    }
}