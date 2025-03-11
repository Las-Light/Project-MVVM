using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.Inventories;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Inventories
{
    public class CmdRemoveGridInventoryHandler : ICommandHandler<CmdRemoveGridInventory>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveGridInventoryHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public bool Handle(CmdRemoveGridInventory command)
        {
            var inventory = _gameState.Inventories.First(inventory => inventory.OwnerId == command.OwnerId);
            var removedInventoryGrid = inventory.InventoryGrids.FirstOrDefault(grid => grid.GridTypeId == command.GridTypeId);

            if (removedInventoryGrid == null)
            {
                Debug.LogError($"Couldn't find InventoryGrid {command.GridTypeId} for ID: {command.OwnerId}");
                return false;
            }
            
            inventory.InventoryGrids.Remove(removedInventoryGrid);
            return true;
        }
    }
}