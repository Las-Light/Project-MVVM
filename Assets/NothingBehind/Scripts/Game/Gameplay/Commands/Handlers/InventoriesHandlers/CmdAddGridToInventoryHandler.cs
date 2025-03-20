using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.InventoriesHandlers
{
    public class CmdAddGridToInventoryHandler : ICommandHandler<CmdAddGridToInventory>
    {
        private readonly GameStateProxy _gameState;

        public CmdAddGridToInventoryHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public bool Handle(CmdAddGridToInventory command)
        {
            var inventory = _gameState.Inventories.First(inventory => inventory.OwnerId == command.OwnerId);
            if (inventory.InventoryGrids.FirstOrDefault(grid => grid.GridId == command.Grid.GridId) != null)
            {
                Debug.LogError(
                    $"InventoryGrid with Id {command.Grid} already exists in Inventory {inventory.OwnerType}-{inventory.OwnerId}.");
                return false;
            }

            if (command.Grid is InventoryGridWithSubGrid subGrid)
            {
                inventory.InventoryGrids.Add(subGrid);
            }
            else
            {
                inventory.InventoryGrids.Add(command.Grid);
            }

            return true;
        }
    }
}