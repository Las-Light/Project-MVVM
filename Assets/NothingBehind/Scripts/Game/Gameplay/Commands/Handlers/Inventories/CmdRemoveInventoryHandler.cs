using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.Inventories;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Inventories
{
    public class CmdRemoveInventoryHandler : ICommandHandler<CmdRemoveInventory>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveInventoryHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public bool Handle(CmdRemoveInventory command)
        {
            var inventories = _gameState.Inventories;
            var removedInventory =
                inventories.FirstOrDefault(inventory => inventory.OwnerId == command.OwnerId);

            if (removedInventory == null)
            {
                Debug.Log($"Couldn't find Inventory for ID: {command.OwnerId}");
                return false;
            }

            inventories.Remove(removedInventory);
            return true;
        }
    }
}