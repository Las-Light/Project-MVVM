using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.View;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.Gameplay.View.Maps;
using NothingBehind.Scripts.Game.Gameplay.View.UI;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventory;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public class WorldGameplayRootViewModel
    {
        private readonly CharactersService _charactersService;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly PlayerService _playerService;
        private readonly ResourcesService _resourcesService;
        private readonly GameplayInputManager _gameplayInputManager;
        private readonly CameraManager _cameraManager;
        private readonly InventoryService _inventoryService;

        public readonly ReadOnlyReactiveProperty<PlayerViewModel> Player;
        public readonly ReadOnlyReactiveProperty<CameraViewModel> CameraViewModel;
        public readonly IObservableCollection<CharacterViewModel> AllCharacters;
        public readonly IObservableCollection<InventoryViewModel> AllInventories;
        public readonly IObservableCollection<MapTransferViewModel> AllMapTransfers;
        public readonly IObservableCollection<EnemySpawnViewModel> AllSpawns;

        public WorldGameplayRootViewModel(CharactersService charactersService,
            IGameStateProvider gameStateProvider,
            PlayerService playerService,
            ResourcesService resourcesService,
            SpawnService spawnService,
            MapTransferService mapService,
            GameplayInputManager gameplayInputManager,
            CameraManager cameraManager,
            InventoryService inventoryService)
        {
            _charactersService = charactersService;
            _gameStateProvider = gameStateProvider;
            _playerService = playerService;
            _resourcesService = resourcesService;
            _gameplayInputManager = gameplayInputManager;
            _cameraManager = cameraManager;
            _inventoryService = inventoryService;

            CameraViewModel = cameraManager.CameraViewModel;
            Player = playerService.PlayerViewModel;
            AllCharacters = charactersService.AllCharacters;
            AllInventories = inventoryService.AllInventories;
            AllMapTransfers = mapService.MapTransfers;
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

            foreach (var inventory in gameState.Inventories)
            {
                if (inventory.OwnerType != EntityType.Player)
                {
                    continue;
                }

                Debug.Log(inventory.OwnerType + " " + inventory.OwnerId);
                foreach (var grid in inventory.InventoryGrids)
                {
                    for (int i = 0; i < grid.Items.Count; i++)
                    {
                        Debug.Log(grid.Items[i].Id + " " + grid.Positions[i]);
                    }
                }
            }

            foreach (var inventoryViewModel in AllInventories)
            {
                if (inventoryViewModel.OwnerType != EntityType.Player)
                {
                    continue;
                }

                foreach (var inventoryGridViewModel in inventoryViewModel.AllInventoryGrids)
                {
                    foreach (var kvp in inventoryGridViewModel.ItemsPositionsMap)
                    {
                        Debug.Log($"In dictionary {kvp.Key.ItemType} {kvp.Key.Id} in position {kvp.Value}");
                    }
                }
            }

            foreach (var inventory in gameState.Inventories)
            {
                if (inventory.OwnerType != EntityType.Player)
                {
                    continue;
                }

                foreach (var grid in inventory.InventoryGrids)
                {
                    for (int i = 0; i < grid.Items.Count; i++)
                    {
                        Debug.Log($"{grid.Items[i].ItemType} {grid.Items[i].Id} {grid.Positions[i]} Stack = {grid.Items[i].CurrentStack.Value}");
                    }

                    for (int i = 0; i < grid.Grid.Value.Length; i++)
                    {
                        Debug.Log(grid.Grid.Value[i] + $" Grid in {inventory.OwnerType} {inventory.OwnerId}");
                    }
                }
            }
        }

        public void HandleTestInputTab()
        {
            // foreach (var inventoryViewModel in _inventoryService.AllInventories)
            // {
            //     if (inventoryViewModel.OwnerTypeId != "Hero")
            //     {
            //         continue;
            //     }
            //
            //     inventoryViewModel.TryMoveItemInGrid( "Backpack", itemProxy2, Vector2Int.zero, 
            //         itemProxy2.CurrentStack.Value);
            // }

            var inventories = _gameStateProvider.GameState._gameState.Inventories;

            foreach (var data in inventories)
            {
                if (data.OwnerType != EntityType.Player)
                {
                    continue;
                }

                foreach (var gridDataProxy in data.InventoryGrids)
                {
                    for (int i = 0; i < gridDataProxy.Items.Count; i++)
                    {
                        Debug.Log(gridDataProxy.GridTypeId + ":");
                        Debug.Log(gridDataProxy.Items[i].ItemType + " ID:" + gridDataProxy.Items[i].Id + " stack:" + gridDataProxy.Items[i].CurrentStack + " " +
                                  gridDataProxy.Positions[i]);
                    }
                }
            }

            foreach (var inventory in AllInventories)
            {
                Debug.Log(inventory.OwnerType + inventory.OwnerId);
            }
            // foreach (var inventory in gameState.Inventories)
            // {
            //     Debug.Log($"Inventory {inventory.OwnerTypeId} - {inventory.OwnerId}");
            //     foreach (var gridDataProxy in inventory.Inventories)
            //     {
            //         Debug.Log($"Entity {inventory.OwnerTypeId} - {inventory.OwnerId} have {gridDataProxy.GridTypeId}");
            //     }
            // }
            //
            // foreach (var inventoryViewModel in AllInventories)
            // {
            //     Debug.Log($"IntoriyVM - {inventoryViewModel.OwnerId}");
            //     foreach (var gridViewModel in inventoryViewModel.AllInventoryGrids)
            //     {
            //         Debug.Log($"GridVM - {gridViewModel.GridTypeID}");
            //     }
            // }
        }
    }
}