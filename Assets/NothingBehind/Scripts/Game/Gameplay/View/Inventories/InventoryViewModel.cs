using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands.Inventories;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventory;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class InventoryViewModel
    {
        public readonly int OwnerId;
        public readonly string OwnerTypeId;
        
        private readonly ICommandProcessor _commandProcessor;
        private readonly InventoryService _inventoryService;

        private readonly ObservableList<InventoryGridViewModel> _allInventoryGrids = new();
        private readonly Dictionary<string, InventoryGridViewModel> _inventoryGridMap = new();
        private readonly Dictionary<string, InventoryGridSettings> _inventoryGridSettingsMap = new();

        public IObservableCollection<InventoryGridViewModel> AllInventoryGrids => _allInventoryGrids;

        public InventoryViewModel(InventoryDataProxy inventoryDataProxy,
            InventorySettings inventorySettings,
            ICommandProcessor commandProcessor,
            InventoryService inventoryService)
        {
            OwnerId = inventoryDataProxy.OwnerId;
            OwnerTypeId = inventoryDataProxy.OwnerTypeId;
            _commandProcessor = commandProcessor;
            _inventoryService = inventoryService;

            foreach (var inventoryGrid in inventorySettings.InventoryGrids)
            {
                _inventoryGridSettingsMap[inventoryGrid.GridTypeId] = inventoryGrid;
            }

            foreach (var inventoryGridDataProxy in inventoryDataProxy.Inventories)
            {
                CreateInventoryGridViewModel(inventoryGridDataProxy);
            }

            inventoryDataProxy.Inventories.ObserveAdd().Subscribe(e =>
            {
                CreateInventoryGridViewModel(e.Value);
            });
            inventoryDataProxy.Inventories.ObserveRemove().Subscribe(e =>
            {
                RemoveInventoryGridViewModel(e.Value);
            });
        }

        public bool CreateGridInventory(string gridTypeId)
        {
            var command = new CmdCreateGridInventory(OwnerTypeId, OwnerId, gridTypeId);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        public bool RemoveGridInventory(string gridTypeId)
        {
            var command = new CmdRemoveGridInventory(OwnerId, gridTypeId);
            var result = _commandProcessor.Process(command);

            return result;
        }
        
        private void CreateInventoryGridViewModel(InventoryGridDataProxy inventoryGridDataProxy)
        {
            var gridSettings = _inventoryGridSettingsMap[inventoryGridDataProxy.GridTypeId];
            var gridViewModel = new InventoryGridViewModel(inventoryGridDataProxy,
                gridSettings);

            _allInventoryGrids.Add(gridViewModel);
            _inventoryGridMap[inventoryGridDataProxy.GridTypeId] = gridViewModel;
        }

        private void RemoveInventoryGridViewModel(InventoryGridDataProxy inventoryGridDataProxy)
        {
            if (_inventoryGridMap.TryGetValue(inventoryGridDataProxy.GridTypeId, out var gridViewModel))
            {
                _allInventoryGrids.Remove(gridViewModel);
                _inventoryGridMap.Remove(inventoryGridDataProxy.GridTypeId);
            }
        }
    }
}