using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.MVVM;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Maps;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Storages;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public class WorldGameplayRootViewModel
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly CharactersService _charactersService;
        private readonly StorageService _storageService;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly PlayerService _playerService;
        private readonly ResourcesService _resourcesService;
        private readonly InputManager _inputManager;
        private readonly CameraService _cameraService;
        private readonly InventoryService _inventoryService;

        public readonly ReadOnlyReactiveProperty<PlayerViewModel> Player;
        public readonly ReadOnlyReactiveProperty<CameraViewModel> CameraViewModel;
        public readonly IObservableCollection<CharacterViewModel> AllCharacters;
        public readonly IObservableCollection<StorageViewModel> AllStorages;
        public readonly IObservableCollection<InventoryViewModel> AllInventories;
        public readonly IObservableCollection<GameplayMapTransferViewModel> AllMapTransfers;
        public readonly IObservableCollection<EnemySpawnViewModel> AllSpawns;

        public WorldGameplayRootViewModel(
            ISettingsProvider settingsProvider,
            CharactersService charactersService,
            StorageService storageService,
            IGameStateProvider gameStateProvider,
            PlayerService playerService,
            ResourcesService resourcesService,
            SpawnService spawnService,
            GameplayMapTransferService gameplayMapService,
            InputManager inputManager,
            CameraService cameraService,
            InventoryService inventoryService)
        {
            _settingsProvider = settingsProvider;
            _charactersService = charactersService;
            _storageService = storageService;
            _gameStateProvider = gameStateProvider;
            _playerService = playerService;
            _resourcesService = resourcesService;
            _inputManager = inputManager;
            _cameraService = cameraService;
            _inventoryService = inventoryService;

            CameraViewModel = cameraService.CameraViewModel;
            Player = playerService.PlayerViewModel;
            AllCharacters = charactersService.AllCharacters;
            AllStorages = storageService.AllStorages;
            AllInventories = inventoryService.AllInventories;
            AllMapTransfers = gameplayMapService.MapTransfers;
            AllSpawns = spawnService.EnemySpawns;

            resourcesService.ObserveResource(ResourceType.SoftCurrency)
                .Subscribe(newValue => Debug.Log($"SoftCurrency: {newValue}"));
            resourcesService.ObserveResource(ResourceType.HardCurrency)
                .Subscribe(newValue => Debug.Log($"HardCurrency: {newValue}"));
        }

        public void HandleTestInput()
        {
            // _charactersService.CreateCharacter(
            //     "Dummy",
            //     Random.Range(1, 3),
            //     new Vector3Int(Random.Range(0, 6), Random.Range(0, 6), Random.Range(0, 6)));
            //
            var gameState = _gameStateProvider.GameState;

            foreach (var inventoryViewModel in AllInventories)
            {
                // if (inventoryViewModel.OwnerType != EntityType.Player)
                // {
                //     continue;
                // }
                Debug.Log("Inv ViewModel " + inventoryViewModel.OwnerType);

                foreach (var inventoryGridViewModel in inventoryViewModel.AllInventoryGrids)
                {
                    Debug.Log(inventoryGridViewModel.GridId + " Type " + inventoryGridViewModel.GridType);
                    foreach (var kvp in inventoryGridViewModel.ItemsPositionsMap)
                    {
                        Debug.Log($"In dictionary {kvp.Key.ItemType} {kvp.Key.Id} in position {kvp.Value}");
                    }
                }
            }

            foreach (var equipment in gameState.Equipments)
            {
                Debug.Log(equipment.OwnerId);

                foreach (var slot in equipment.Slots)
                {
                    Debug.Log($"Slot - {slot.SlotType} have item - {slot.EquippedItem.Value}");
                }
            }
        }

        public void HandleTestInputTab()
        {
            var gameState = _gameStateProvider.GameState;
            foreach (var inventory in AllInventories)
            {
                var randomInt = Random.Range(0, 7);
                Debug.Log(randomInt);
                var itemSettings = _settingsProvider.GameSettings.ItemsSettings.Items[randomInt];
                //var itemSettings = _settingsProvider.GameSettings.ItemsSettings.Items.First(settings => settings.WeaponName == WeaponName.Glock);
                var gameStateData = _gameStateProvider.GameState.GameState;
                var item = ItemsFactory.CreateItem(ItemsDataFactory.CreateItemData(
                    gameStateData, _settingsProvider.GameSettings, itemSettings));
                foreach (var grid in inventory.AllInventoryGrids)
                {
                    if (grid.GridType == InventoryGridType.Backpack)
                    {
                        var result = grid.AddItems(item, item.CurrentStack.Value);
                        if (result.Success)
                        {
                            Debug.Log(item.ItemType);
                        }
                    }
                }
            }
        }
    }
}