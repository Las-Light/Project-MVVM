using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Maps;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Storages;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Camera;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Player;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Transfers;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Root.View
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
        private readonly EquipmentService _equipmentService;

        public readonly Dictionary<int, ArsenalViewModel> ArsenalsMap;
        public readonly Dictionary<int, InventoryViewModel> InventoriesMap;
        public readonly ReadOnlyReactiveProperty<PlayerViewModel> Player;
        public readonly ReadOnlyReactiveProperty<CameraViewModel> CameraViewModel;
        public readonly ObservableList<MapTransferViewModel> AllMapTransfers;
        public readonly IObservableCollection<CharacterViewModel> AllCharacters;
        public readonly IObservableCollection<StorageViewModel> AllStorages;
        public readonly IObservableCollection<EquipmentViewModel> AllEquipments;
        public readonly IObservableCollection<InventoryViewModel> AllInventories;
        public readonly IObservableCollection<EnemySpawnViewModel> AllSpawns;


        public WorldGameplayRootViewModel(
            ISettingsProvider settingsProvider,
            CharactersService charactersService,
            StorageService storageService,
            IGameStateProvider gameStateProvider,
            PlayerService playerService,
            ResourcesService resourcesService,
            SpawnService spawnService,
            MapTransferService mapService,
            InputManager inputManager,
            CameraService cameraService,
            InventoryService inventoryService,
            EquipmentService equipmentService,
            ArsenalService arsenalService)
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
            _equipmentService = equipmentService;

            CameraViewModel = cameraService.CameraViewModel;
            Player = playerService.PlayerViewModel;
            AllCharacters = charactersService.AllCharacters;
            AllStorages = storageService.AllStorages;
            AllInventories = inventoryService.AllInventories;
            ArsenalsMap = arsenalService.ArsenalMap;
            InventoriesMap = inventoryService.InventoryMap;
            AllEquipments = equipmentService.AllEquipmentViewModels;
            AllSpawns = spawnService.EnemySpawns;

            var gameState = _gameStateProvider.GameState;

            if (mapService.AllTransfersMaps.TryGetValue(gameState.CurrentMapId.Value, out var mapTransferViewModels))
            {
                AllMapTransfers = mapTransferViewModels;

                mapTransferViewModels.ObserveAdd().Subscribe(e =>
                {
                    AllMapTransfers.Add(e.Value);
                });
                mapTransferViewModels.ObserveRemove().Subscribe(e =>
                {
                    AllMapTransfers.Remove(e.Value);
                });
            }

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
                Debug.Log("Inv ViewModel " + inventoryViewModel.OwnerType + ", ID - " + inventoryViewModel.OwnerId);

                // foreach (var inventoryGridViewModel in inventoryViewModel.AllInventoryGrids)
                // {
                //     Debug.Log(inventoryGridViewModel.GridId + " Type " + inventoryGridViewModel.GridType);
                //     foreach (var kvp in inventoryGridViewModel.ItemsPositionsMap)
                //     {
                //         Debug.Log($"In dictionary {kvp.Key.ItemType} {kvp.Key.Id} in position {kvp.Value}");
                //     }
                // }
            }

            foreach (var arsenalViewModel in ArsenalsMap)
            {
                Debug.Log($"Arsenal ViewModel - {arsenalViewModel.Value.OwnerId}, Type - {arsenalViewModel.Value.OwnerType}");
            }

            foreach (var equipmentSlot in gameState.Player.Value.Equipment.Value.Slots)
            {
                //Debug.Log(equipmentSlot.EquippedItem.Value.ItemType);
            }

            foreach (var equipment in AllEquipments)
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
            _resourcesService.AddResources(ResourceType.HardCurrency, 10);
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