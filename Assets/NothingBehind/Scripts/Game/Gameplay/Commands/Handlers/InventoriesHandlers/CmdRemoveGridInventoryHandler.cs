using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.InventoriesHandlers
{
    public class CmdRemoveGridInventoryHandler : ICommandHandler<CmdRemoveGridFromInventory>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveGridInventoryHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public bool Handle(CmdRemoveGridFromInventory command)
        {
            var inventory = _gameState.Inventories.First(inventory => inventory.OwnerId == command.OwnerId);
            var removedInventoryGrid = inventory.InventoryGrids.FirstOrDefault(grid => grid.GridId == command.Grid.GridId);

            if (removedInventoryGrid == null)
            {
                Debug.LogError($"Couldn't find InventoryGrid with ID: {command.Grid} for owner with ID: {command.OwnerId}");
                return false;
            }
            
            inventory.InventoryGrids.Remove(removedInventoryGrid);
            return true;
        }
    }
}