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
    public class CmdCreateGridInventoryHandler : ICommandHandler<CmdCreateGridInventory>
    {
        private readonly GameStateProxy _gameState;
        private readonly InventoriesSettings _inventoriesSettings;

        public CmdCreateGridInventoryHandler(GameStateProxy gameState, InventoriesSettings inventoriesSettings)
        {
            _gameState = gameState;
            _inventoriesSettings = inventoriesSettings;
        }
        public bool Handle(CmdCreateGridInventory command)
        {
            var inventorySettings =
                _inventoriesSettings.Inventories.First(settings => settings.OwnerTypeId == command.OwnerTypeId);
            var inventoryGridSettings =
                inventorySettings.InventoryGrids.First(settings => settings.GridTypeId == command.GridTypeId);
            var inventory = _gameState.Inventories.First(inventoryProxy => inventoryProxy.OwnerId == command.OwnerId);
            if (inventory.Inventories.FirstOrDefault(grid => grid.GridTypeId == command.GridTypeId) != null)
            {
                Debug.LogError($"InventoryGrid with Type {inventoryGridSettings.GridTypeId} already exists in Inventory {inventory.OwnerTypeId}-{inventory.OwnerId}.");
                return false;
            }
            
            var inventoryGrid = new InventoryGridData()
            {
                OwnerId = command.OwnerId,
                GridTypeId = inventoryGridSettings.GridTypeId,
                Width = inventoryGridSettings.Width,
                Height = inventoryGridSettings.Height,
                Grid = new bool[inventoryGridSettings.Width * inventoryGridSettings.Height],
                Items = new List<ItemData>(),
                Positions = new List<Vector2Int>()
            };

            var inventoryGridProxy = new InventoryGridDataProxy(inventoryGrid);
            inventory.Inventories.Add(inventoryGridProxy);

            return true;
        }
    }
}