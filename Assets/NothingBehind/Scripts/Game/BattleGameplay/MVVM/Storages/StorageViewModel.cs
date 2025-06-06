using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Storages;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Storages
{
    public class StorageViewModel
    {
        private readonly Storage _storage;
        private readonly StorageSettings _storageSettings;
        private readonly StorageService _storageService;
        private readonly InventoryViewModel _inventoryViewModel;

        public readonly int Id;
        public readonly EntityType EntityType;
        public ReadOnlyReactiveProperty<Vector3> Position { get; }

        public StorageViewModel(Storage storage, StorageSettings storageSettings, StorageService storageService,
            InventoryViewModel inventoryViewModel)
        {
            _storage = storage;
            _storageSettings = storageSettings;
            _storageService = storageService;
            _inventoryViewModel = inventoryViewModel;

            Id = storage.Id;
            EntityType = storage.EntityType;
            Position = storage.Position;
        }

        public bool IsEmptyInventory()
        {
            return _inventoryViewModel.IsEmptyInventory();
        }
    }
}