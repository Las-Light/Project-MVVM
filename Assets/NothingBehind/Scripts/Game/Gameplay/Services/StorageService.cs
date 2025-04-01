using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands.StoragesCommands;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Storages;
using NothingBehind.Scripts.Game.Settings.Gameplay.Storages;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class StorageService
    {
        private readonly InventoryService _inventoryService;
        private readonly ICommandProcessor _commandProcessor;
        private readonly ObservableList<StorageViewModel> _allStorages = new();
        private readonly Dictionary<int, StorageViewModel> _storagesMap = new();
        private readonly Dictionary<EntityType, StorageSettings> _storageSettingsMap = new();

        public IObservableCollection<StorageViewModel> AllStorages => _allStorages;
        public Dictionary<int, StorageViewModel> StorageMap => _storagesMap;

        public StorageService(
            IObservableCollection<Storage> storages,
            StoragesSettings storagesSettings,
            InventoryService inventoryService,
            ICommandProcessor commandProcessor,
            Subject<ExitInventoryRequestResult> exitInventorRequest)
        {
            _inventoryService = inventoryService;
            _commandProcessor = commandProcessor;

            foreach (var storageSettings in storagesSettings.Storages)
            {
                _storageSettingsMap[storageSettings.EntityType] = storageSettings;
            }

            foreach (var storage in storages)
            {
                CreateStorageViewModel(storage);
            }

            storages.ObserveAdd().Subscribe(e => { CreateStorageViewModel(e.Value); });

            storages.ObserveRemove().Subscribe(e => { RemoveStorageViewModel(e.Value); });

            // Когда приходит реквест, то StorageService удаляет из GameState этот Storage (реквест приходит из InventoryUIView при его удалении)
            exitInventorRequest.Where(result => result.IsEmptyInventory && result.EntityType == EntityType.Storage)
                .Subscribe(result =>
            {
                RemoveStorage(result.OwnerId);
            });
        }

        public CommandResult CreateStorage(EntityType entityType, Vector3 position)
        {
            var command = new CmdCreateStorage(entityType, position, _inventoryService);
            var result = _commandProcessor.Process(command);

            return result;
        }

        public CommandResult RemoveStorage(int id)
        {
            var command = new CmdRemoveStorage(id, _inventoryService);
            var result = _commandProcessor.Process(command);

            return result;
        }

        private void CreateStorageViewModel(Storage storage)
        {
            var storageSettings = _storageSettingsMap[storage.EntityType];
            if (_inventoryService.InventoryMap.TryGetValue(storage.Id, out var inventoryViewModel))
            {
                Debug.LogError($"Inventory with Id - {storage.Id} not found");
            }

            var storageViewModel = new StorageViewModel(storage,
                storageSettings,
                this,
                inventoryViewModel);

            _allStorages.Add(storageViewModel);
            _storagesMap[storage.Id] = storageViewModel;
        }

        private void RemoveStorageViewModel(Storage storage)
        {
            if (_storagesMap.TryGetValue(storage.Id, out var storageViewModel))
            {
                _allStorages.Remove(storageViewModel);
                _storagesMap.Remove(storage.Id);
            }
        }
    }
}