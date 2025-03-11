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
            foreach (var inventoryGridSettings in inventorySettings.InventoryGrids)
            {
                var subGrids = CreateSubGrids(command.OwnerId, inventoryGridSettings.SubGrids);
                var items = CreateItems(inventoryGridSettings.Items);

                inventoryGrids.Add(new InventoryGridData(command.OwnerId,
                    inventoryGridSettings.GridTypeId,
                    inventoryGridSettings.Width,
                    inventoryGridSettings.Height,
                    inventoryGridSettings.CellSize,
                    inventoryGridSettings.IsSubGrid,
                    subGrids,
                    items));
            }

            inventory.InventoryGrids = inventoryGrids;

            var inventoryProxy = new InventoryDataProxy(inventory);
            _gameState.Inventories.Add(inventoryProxy);

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