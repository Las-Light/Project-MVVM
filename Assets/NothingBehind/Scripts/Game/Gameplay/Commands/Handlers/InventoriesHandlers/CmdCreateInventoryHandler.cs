using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.InventoriesHandlers
{
    public class CmdCreateInventoryHandler : ICommandHandler<CmdCreateInventory>
    {
        private readonly GameStateProxy _gameState;
        private readonly InventoriesSettings _inventoriesSettings;

        public CmdCreateInventoryHandler(GameStateProxy gameState,
            InventoriesSettings inventoriesSettings)
        {
            _gameState = gameState;
            _inventoriesSettings = inventoriesSettings;
        }

        public bool Handle(CmdCreateInventory command)
        {
            var inventorySettings =
                _inventoriesSettings.Inventories.First(settings => settings.OwnerType == command.OwnerType);
            var inventory = new InventoryData()
            {
                OwnerId = command.OwnerId,
                OwnerType = command.OwnerType
            };

            var inventoryGrids = new List<InventoryGridData>();

            inventory.InventoryGrids = inventoryGrids;

            var inventoryProxy = new Inventory(inventory);
            _gameState.Inventories.Add(inventoryProxy);

            return true;
        }
    }
}