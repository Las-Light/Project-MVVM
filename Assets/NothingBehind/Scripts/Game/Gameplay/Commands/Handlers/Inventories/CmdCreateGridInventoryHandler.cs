using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Game.State.Items;
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
                _inventoriesSettings.Inventories.First(settings => settings.OwnerType == command.OwnerType);
            var inventoryGridSettings =
                inventorySettings.InventoryGrids.First(settings => settings.GridTypeId == command.GridTypeId);
            var inventory = _gameState.Inventories.First(inventoryProxy => inventoryProxy.OwnerId == command.OwnerId);
            if (inventory.InventoryGrids.FirstOrDefault(grid => grid.GridTypeId == command.GridTypeId) != null)
            {
                Debug.LogError(
                    $"InventoryGrid with Type {inventoryGridSettings.GridTypeId} already exists in Inventory {inventory.OwnerType}-{inventory.OwnerId}.");
                return false;
            }

            var subGrids = CreateSubGrids(command.OwnerId, inventoryGridSettings.SubGrids);
            var items = CreateItems(inventoryGridSettings.Items);

            var inventoryGrid = new InventoryGridData(command.OwnerId,
                inventoryGridSettings.GridTypeId,
                inventoryGridSettings.Width,
                inventoryGridSettings.Height,
                inventoryGridSettings.CellSize,
                inventoryGridSettings.IsSubGrid,
                subGrids,
                items);

            var inventoryGridProxy = new InventoryGrid(inventoryGrid);
            inventory.InventoryGrids.Add(inventoryGridProxy);

            return true;
        }

        private List<InventoryGridData> CreateSubGrids(int ownerId, List<InventoryGridSettings> subGrids)
        {
            var inventorySubGrids = new List<InventoryGridData>();
            foreach (var subGridSettings in subGrids)
            {
                var subGridData = new InventoryGridData(ownerId,
                    subGridSettings.GridTypeId,
                    subGridSettings.Width,
                    subGridSettings.Height,
                    subGridSettings.CellSize,
                    subGridSettings.IsSubGrid,
                    new List<InventoryGridData>(),
                    CreateItems(subGridSettings.Items));
                inventorySubGrids.Add(subGridData);
            }

            return inventorySubGrids;
        }

        private List<ItemData> CreateItems(List<ItemSettings> itemsSettings)
        {
            var items = new List<ItemData>();
            foreach (var itemSettings in itemsSettings)
            {
                var item = new ItemData(_gameState.CreateItemId(),
                    itemSettings.ItemType,
                    itemSettings.Width,
                    itemSettings.Height,
                    itemSettings.Weight,
                    itemSettings.CanRotate,
                    itemSettings.IsRotated,
                    itemSettings.IsStackable,
                    itemSettings.MaxStackSize,
                    itemSettings.CurrentStack);
                items.Add(item);
            }

            return items;
        }
    }
}