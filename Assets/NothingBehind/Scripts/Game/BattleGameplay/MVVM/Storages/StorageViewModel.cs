using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Storages;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Storages
{
    public class StorageViewModel
    {
        private readonly StorageEntity _storageEntity;
        private readonly StorageSettings _storageSettings;
        private readonly StorageService _storageService;
        private readonly InventoryViewModel _inventoryViewModel;

        public readonly int Id;
        public readonly EntityType EntityType;
        public ReadOnlyReactiveProperty<Vector3> Position { get; }

        public StorageViewModel(StorageEntity storageEntity, 
            StorageSettings storageSettings, 
            StorageService storageService,
            InventoryViewModel inventoryViewModel)
        {
            _storageEntity = storageEntity;
            _storageSettings = storageSettings;
            _storageService = storageService;
            _inventoryViewModel = inventoryViewModel;

            Id = storageEntity.UniqueId;
            EntityType = storageEntity.EntityType;
            Position = storageEntity.Position;
        }

        public bool IsEmptyInventory()
        {
            return _inventoryViewModel.IsEmptyInventory();
        }
    }
}