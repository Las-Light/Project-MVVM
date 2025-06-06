using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.GameRoot.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.InventoriesHandlers
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

        public CommandResult Handle(CmdCreateInventory command)
        {
            var inventorySettings =
                _inventoriesSettings.Inventories.First(settings => settings.OwnerType == command.OwnerType);
            var inventoryData = new InventoryData()
            {
                OwnerId = command.OwnerId,
                OwnerType = command.OwnerType
            };

            var inventoryGrids = new List<InventoryGridData>();

            // Для сущностей у которых есть инвентарь, но нет EquipmentSystem создаем сетки
            if (command.OwnerType == EntityType.Storage)
            {
                var gridsSettings = inventorySettings.GridsSettings;
                foreach (var gridSettings in gridsSettings)
                {
                    var grid = InventoryGridsDataFactory.CreateInventorGridData(_gameState.GameState, gridSettings);
                    inventoryGrids.Add(grid);
                }
            }

            inventoryData.InventoryGrids = inventoryGrids;
            var inventory = new Inventory(inventoryData);
            _gameState.Inventories.Add(inventory);

            return new CommandResult(command.OwnerId, true);
        }
    }
}