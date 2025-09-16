using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using NothingBehind.Scripts.Game.State.Inventories;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.BattleGameplay.Services
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

        public InventoryService(ReadOnlyReactiveProperty<PlayerEntity> playerEntity,
            IObservableCollection<Entity> entities,
            EquipmentService equipmentService,
            InventoriesSettings inventoriesSettings,
            ItemsSettings itemsSettings,
            ICommandProcessor commandProcessor)
        {
            _equipmentService = equipmentService;
            _itemsSettings = itemsSettings;
            _commandProcessor = commandProcessor;

            foreach (var inventorySettings in inventoriesSettings.Inventories)
            {
                _inventorySettingsMap[inventorySettings.OwnerType] = inventorySettings;
            }
            
            PlayerId = playerEntity.CurrentValue.UniqueId;
            var playerInventory = playerEntity.CurrentValue.Inventory.Value;
            _inventoryDataMap[PlayerId] = playerInventory;
            CreateInventoryViewModel(PlayerId);
            
            //TODO: Создать общий класс сущностей которые имеют инвентарь

            foreach (var entity in entities)
            {
                if (entity is CharacterEntity characterEntity)
                {
                    var characterInventory = characterEntity.Inventory.Value;
                    _inventoryDataMap[characterInventory.OwnerId] = characterInventory;
                    CreateInventoryViewModel(characterInventory.OwnerId);
                }

                if (entity is StorageEntity storageEntity)
                {
                    var storageInventory = storageEntity.Inventory.Value;
                    _inventoryDataMap[storageInventory.OwnerId] = storageInventory;
                    CreateInventoryViewModel(storageInventory.OwnerId);
                }
            }

            entities.ObserveAdd().Subscribe(e =>
            {
                var addedEntity = e.Value;
                if (addedEntity is CharacterEntity characterEntity)
                {
                    var inventory = characterEntity.Inventory.Value;
                    _inventoryDataMap[inventory.OwnerId] = inventory;
                    CreateInventoryViewModel(inventory.OwnerId);
                }

                if (addedEntity is StorageEntity storageEntity)
                {
                    var storageInventory = storageEntity.Inventory.Value;
                    _inventoryDataMap[storageInventory.OwnerId] = storageInventory;
                    CreateInventoryViewModel(storageInventory.OwnerId);
                }
            }).AddTo(_disposables);
            entities.ObserveRemove().Subscribe(e =>
            {
                var removedEntity = e.Value;
                if (removedEntity is CharacterEntity characterEntity)
                {
                    var inventory = characterEntity.Inventory.Value;
                    _inventoryDataMap.Remove(removedEntity.UniqueId);
                    RemoveInventoryViewModel(inventory);
                }

                if (removedEntity is StorageEntity storageEntity)
                {
                    var inventory = storageEntity.Inventory.Value;
                    _inventoryDataMap.Remove(removedEntity.UniqueId);
                    RemoveInventoryViewModel(inventory);
                }
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