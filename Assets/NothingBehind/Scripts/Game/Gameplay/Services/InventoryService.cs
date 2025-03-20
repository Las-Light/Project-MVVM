using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Inventories;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class InventoryService
    {
        public int PlayerId { get; }
        private readonly EquipmentService _equipmentService;
        private readonly ICommandProcessor _commandProcessor;

        private readonly ObservableList<InventoryViewModel> _allInventories = new();
        private readonly Dictionary<int, InventoryViewModel> _inventoryMap = new();
        private readonly Dictionary<int, Inventory> _inventoryDataMap = new();
        private readonly Dictionary<EntityType, InventorySettings> _inventorySettingsMap = new();

        public IObservableCollection<InventoryViewModel> AllInventories => _allInventories;

        public Dictionary<int, InventoryViewModel> InventoryMap => _inventoryMap;

        public InventoryService(IObservableCollection<Inventory> inventories,
            EquipmentService equipmentService,
            InventoriesSettings inventoriesSettings,
            ICommandProcessor commandProcessor, int playerId)
        {
            PlayerId = playerId;
            _equipmentService = equipmentService;
            _commandProcessor = commandProcessor;

            foreach (var inventorySettings in inventoriesSettings.Inventories)
            {
                _inventorySettingsMap[inventorySettings.OwnerType] = inventorySettings;
            }

            foreach (var inventory in inventories)
            {
                _inventoryDataMap[inventory.OwnerId] = inventory;
            }

            inventories.ObserveAdd().Subscribe(e =>
            {
                _inventoryDataMap[e.Value.OwnerId] = e.Value;
            });
            inventories.ObserveRemove().Subscribe(e =>
            {
                _inventoryDataMap.Remove(e.Value.OwnerId);
            });
        }

        public bool CreateInventory(EntityType ownerType, int ownerId)
        {
            var command = new CmdCreateInventory(ownerType, ownerId);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        public bool RemoveInventory(int ownerId)
        {
            var command = new CmdRemoveInventory(ownerId);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        public InventoryViewModel CreateInventoryViewModel(int ownerId)
        {
            if (_inventoryDataMap.TryGetValue(ownerId, out var inventory))
            {
                var inventorySettings = _inventorySettingsMap[inventory.OwnerType];
                var inventoryViewModel = new InventoryViewModel(inventory,
                    _equipmentService,
                    inventorySettings,
                    _commandProcessor,
                    this);

                _allInventories.Add(inventoryViewModel);
                _inventoryMap[inventory.OwnerId] = inventoryViewModel;
                return inventoryViewModel;
            }

            return null;
        }

        public void RemoveInventoryViewModel(Inventory inventory)
        {
            if (_inventoryMap.TryGetValue(inventory.OwnerId, out var inventoryViewModel))
            {
                _allInventories.Remove(inventoryViewModel);
                _inventoryMap.Remove(inventory.OwnerId);
                inventoryViewModel.Dispose();
            }
        }
    }
}