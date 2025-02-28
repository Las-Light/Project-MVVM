using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Inventories
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
                _inventoriesSettings.Inventories.First(settings => settings.OwnerTypeId == command.OwnerTypeId);
            var inventory = new InventoryData()
            {
                OwnerId = command.OwnerId,
                OwnerTypeId = command.OwnerTypeId
            };
            var inventoryGrids = new List<InventoryGridData>();
            foreach (var inventoryGrid in inventorySettings.InventoryGrids)
            {
                inventoryGrids.Add(new InventoryGridData()
                {
                    GridTypeId = inventoryGrid.GridTypeId,
                    Width = inventoryGrid.Width,
                    Height = inventoryGrid.Height,
                    Grid = new bool[inventoryGrid.Width * inventoryGrid.Height],
                    Items = new List<ItemData>(),
                    Positions = new List<Vector2Int>()
                });
            }
            inventory.Inventories = inventoryGrids;

            var inventoryProxy = new InventoryDataProxy(inventory);
            _gameState.Inventories.Add(inventoryProxy);

            return true;
        }
    }
}