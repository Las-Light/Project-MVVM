using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands.Inventories;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventory;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class InventoryService
    {
        private readonly ICommandProcessor _commandProcessor;

        private readonly ObservableList<InventoryViewModel> _allInventories = new();
        private readonly Dictionary<int, InventoryViewModel> _inventoryMap = new();
        private readonly Dictionary<string, InventorySettings> _inventorySettingsMap = new();

        public IObservableCollection<InventoryViewModel> AllInventories => _allInventories;
        
        public InventoryService(IObservableCollection<InventoryDataProxy> inventories,
            InventoriesSettings inventoriesSettings,
            ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;

            foreach (var inventorySettings in inventoriesSettings.Inventories)
            {
                _inventorySettingsMap[inventorySettings.OwnerTypeId] = inventorySettings;
            }

            foreach (var inventoryDataProxy in inventories)
            {
                CreateInventoryViewModel(inventoryDataProxy);
            }

            inventories.ObserveAdd().Subscribe(e =>
            {
                CreateInventoryViewModel(e.Value);
            });
            inventories.ObserveRemove().Subscribe(e =>
            {
                RemoveInventoryViewModel(e.Value);
            });
        }

        public bool CreateInventory(string ownerTypeId, int ownerId)
        {
            var command = new CmdCreateInventory(ownerTypeId, ownerId);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        public bool RemoveInventory(int ownerId)
        {
            var command = new CmdRemoveInventory(ownerId);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        private void CreateInventoryViewModel(InventoryDataProxy inventoryDataProxy)
        {
            var inventorySettings = _inventorySettingsMap[inventoryDataProxy.OwnerTypeId];
            var inventoryViewModel = new InventoryViewModel(inventoryDataProxy,
                inventorySettings,
                _commandProcessor,
                this);

            _allInventories.Add(inventoryViewModel);
            _inventoryMap[inventoryDataProxy.OwnerId] = inventoryViewModel;
        }

        private void RemoveInventoryViewModel(InventoryDataProxy inventoryDataProxy)
        {
            if (_inventoryMap.TryGetValue(inventoryDataProxy.OwnerId, out var inventoryViewModel))
            {
                _allInventories.Remove(inventoryViewModel);
                _inventoryMap.Remove(inventoryDataProxy.OwnerId);
            }
        }
    }
}