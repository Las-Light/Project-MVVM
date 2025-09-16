using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.EntityCommands;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Storages;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Storages;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Services
{
    public class StorageService
    {
        public IObservableCollection<StorageViewModel> AllStorages => _allStorages;
        public Dictionary<int, StorageViewModel> StorageMap => _storagesMap;
        
        private readonly InventoryService _inventoryService;
        private readonly ICommandProcessor _commandProcessor;
        private readonly ObservableList<StorageViewModel> _allStorages = new();
        private readonly Dictionary<int, StorageViewModel> _storagesMap = new();
        private readonly Dictionary<EntityType, StorageSettings> _storageSettingsMap = new();

        private readonly CompositeDisposable _disposables = new();

        public StorageService(
            IObservableCollection<Entity> entities,
            StoragesSettings storagesSettings,
            InventoryService inventoryService,
            ICommandProcessor commandProcessor,
            Subject<ExitInventoryRequestResult> exitInventoryRequest)
        {
            _inventoryService = inventoryService;
            _commandProcessor = commandProcessor;

            foreach (var storageSettings in storagesSettings.Storages)
            {
                _storageSettingsMap[storageSettings.EntityType] = storageSettings;
            }

            foreach (var entity in entities)
            {
                if (entity is StorageEntity storageEntity)
                {
                    CreateStorageViewModel(storageEntity);
                }
            }

            entities.ObserveAdd().Subscribe(e =>
            {
                var addedEntity = e.Value;
                if (addedEntity is StorageEntity storageEntity)
                {
                    CreateStorageViewModel(storageEntity);
                }
            }).AddTo(_disposables);
            entities.ObserveRemove().Subscribe(e =>
            {
                var removedEntity = e.Value;
                if (removedEntity is StorageEntity storageEntity)
                {
                    RemoveStorageViewModel(storageEntity);
                }
            }).AddTo(_disposables);

            // Когда приходит реквест, то StorageService удаляет из GameState этот Storage (реквест приходит из InventoryUIView при его удалении)
            exitInventoryRequest.Where(result => result.IsEmptyInventory && result.EntityType == EntityType.Storage)
                .Subscribe(result =>
            {
                RemoveEntity(result.OwnerId);
            });
        }

        public CommandResult CreateEntity(EntityType entityType, string configId, int level, Vector3 position)
        {
            var command = new CmdCreateEntity(entityType, configId, level, position);
            var result = _commandProcessor.Process(command);

            return result;
        }

        public CommandResult RemoveEntity(int entityId)
        {
            var command = new CmdRemoveEntity(entityId);
            var result = _commandProcessor.Process(command);

            return result;
        }
        
        private void CreateStorageViewModel(StorageEntity storageEntity)
        {
            var storageSettings = _storageSettingsMap[storageEntity.EntityType];
            if (!_inventoryService.InventoryMap.TryGetValue(storageEntity.UniqueId, out var inventoryViewModel))
            {
                Debug.LogError($"Inventory with Id - {storageEntity.UniqueId} not found");
            }

            var storageViewModel = new StorageViewModel(storageEntity,
                storageSettings,
                this,
                inventoryViewModel);

            _allStorages.Add(storageViewModel);
            _storagesMap[storageEntity.UniqueId] = storageViewModel;
        }

        private void RemoveStorageViewModel(StorageEntity storageEntity)
        {
            if (_storagesMap.TryGetValue(storageEntity.UniqueId, out var storageViewModel))
            {
                _allStorages.Remove(storageViewModel);
                _storagesMap.Remove(storageEntity.UniqueId);
            }
        }
    }
}