using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.GameRoot.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.GameRoot.Services
{
    public class InventoryService : IDisposable
    {
        public int PlayerId { get; }
        public Dictionary<int, InventoryViewModel> InventoryMap => _inventoryMap;
        public IObservableCollection<InventoryViewModel> AllInventories => _allInventories;

        private readonly EquipmentService _equipmentService;
        private readonly ItemsSettings _itemsSettings;
        private readonly ICommandProcessor _commandProcessor;

        private readonly ObservableList<InventoryViewModel> _allInventories = new();
        private readonly Dictionary<int, InventoryViewModel> _inventoryMap = new();
        private readonly Dictionary<int, Inventory> _inventoryDataMap = new();
        private readonly Dictionary<EntityType, InventorySettings> _inventorySettingsMap = new();
        private CompositeDisposable _disposables = new();

        public InventoryService(IObservableCollection<Inventory> inventories,
            EquipmentService equipmentService,
            InventoriesSettings inventoriesSettings,
            ItemsSettings itemsSettings,
            ICommandProcessor commandProcessor, int playerId)
        {
            PlayerId = playerId;
            _equipmentService = equipmentService;
            _itemsSettings = itemsSettings;
            _commandProcessor = commandProcessor;

            foreach (var inventorySettings in inventoriesSettings.Inventories)
            {
                _inventorySettingsMap[inventorySettings.OwnerType] = inventorySettings;
            }

            foreach (var inventory in inventories)
            {
                _inventoryDataMap[inventory.OwnerId] = inventory;
                CreateInventoryViewModel(inventory.OwnerId);
            }

            inventories.ObserveAdd().Subscribe(e =>
            {
                var addedInventory = e.Value;
                _inventoryDataMap[addedInventory.OwnerId] = addedInventory;
                CreateInventoryViewModel(addedInventory.OwnerId);
            }).AddTo(_disposables);
            inventories.ObserveRemove().Subscribe(e =>
            {
                var removedInventory = e.Value;
                _inventoryDataMap.Remove(removedInventory.OwnerId);
                RemoveInventoryViewModel(removedInventory);
            }).AddTo(_disposables);
        }

        public void UpdateInventories(IObservableCollection<Inventory> newInventories)
        {
            // Очищаем данные и отписываемся от старой коллекции
            ClearCurrentData();
            _disposables.Dispose();
            _disposables = new CompositeDisposable();

            // Обновляем ссылку и подписываемся на новую коллекцию
            foreach (var inventory in newInventories)
            {
                _inventoryDataMap[inventory.OwnerId] = inventory;
                CreateInventoryViewModel(inventory.OwnerId);
            }

            newInventories.ObserveAdd().Subscribe(e =>
            {
                var addedInventory = e.Value;
                _inventoryDataMap[addedInventory.OwnerId] = addedInventory;
                CreateInventoryViewModel(addedInventory.OwnerId);
            }).AddTo(_disposables);

            newInventories.ObserveRemove().Subscribe(e =>
            {
                var removedInventory = e.Value;
                _inventoryDataMap.Remove(removedInventory.OwnerId);
                RemoveInventoryViewModel(removedInventory);
            }).AddTo(_disposables);
        }

        public CommandResult CreateInventory(EntityType ownerType, int ownerId)
        {
            var command = new CmdCreateInventory(ownerType, ownerId);
            var result = _commandProcessor.Process(command);

            return result;
        }

        public CommandResult RemoveInventory(int ownerId)
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
                    _itemsSettings,
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

        private void ClearCurrentData()
        {
            foreach (var inventory in _inventoryDataMap.Values)
            {
                RemoveInventoryViewModel(inventory);
            }

            _inventoryDataMap.Clear();
            _allInventories.Clear();
            _inventoryMap.Clear();
        }

        public void Dispose()
        {
            ClearCurrentData();
            _disposables.Dispose();
        }
    }
}