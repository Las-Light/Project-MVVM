using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.Services.Hero;
using NothingBehind.Scripts.Game.Gameplay.View;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.Gameplay.View.Maps;
using NothingBehind.Scripts.Game.State;
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
        private readonly HeroService _heroService;
        private readonly ResourcesService _resourcesService;
        private readonly GameplayInputManager _gameplayInputManager;
        private readonly CameraManager _cameraManager;
        private readonly InventoryService _inventoryService;

        public readonly ReadOnlyReactiveProperty<HeroViewModel> Hero;
        public readonly ReadOnlyReactiveProperty<CameraViewModel> CameraViewModel;
        public readonly IObservableCollection<CharacterViewModel> AllCharacters;
        public readonly IObservableCollection<InventoryViewModel> AllInventories;
        public readonly IObservableCollection<MapTransferViewModel> AllMapTransfers;
        public readonly IObservableCollection<EnemySpawnViewModel> AllSpawns;

        public WorldGameplayRootViewModel(CharactersService charactersService,
            IGameStateProvider gameStateProvider,
            HeroService heroService,
            ResourcesService resourcesService,
            SpawnService spawnService,
            MapTransferService mapService,
            GameplayInputManager gameplayInputManager,
            CameraManager cameraManager,
            InventoryService inventoryService)
        {
            _charactersService = charactersService;
            _gameStateProvider = gameStateProvider;
            _heroService = heroService;
            _resourcesService = resourcesService;
            _gameplayInputManager = gameplayInputManager;
            _cameraManager = cameraManager;
            _inventoryService = inventoryService;

            CameraViewModel = cameraManager.CameraViewModel;
            Hero = heroService.HeroViewModel;
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

            var item = new ItemData()
            {
                Id = "Sword",
                Height = 1,
                Width = 1,
                CanRotate = false,
                IsRotated = false
            };
            var item2 = new ItemData()
            {
                Id = "Gun",
                Height = 1,
                Width = 1,
                CanRotate = false,
                IsRotated = false
            };
            var itemProxy = new ItemDataProxy(item);
            var itemProxy2 = new ItemDataProxy(item2);
            foreach (var inventoryViewModel in _inventoryService.AllInventories)
            {
                if (inventoryViewModel.OwnerTypeId != "Hero")
                {
                    continue;
                }

                foreach (var inventoryGridViewModel in inventoryViewModel.AllInventoryGrids)
                {
                    inventoryGridViewModel.AddItem(itemProxy, Vector2Int.zero);
                    inventoryGridViewModel.AddItem(itemProxy2);
                }
            }

            foreach (var inventory in gameState.Inventories)
            {
                Debug.Log(inventory.OwnerTypeId + " " + inventory.OwnerId);
                foreach (var grid in inventory.Inventories)
                {
                    for (int i = 0; i < grid.Items.Count; i++)
                    {
                        Debug.Log(grid.Items[i].Id + " " + grid.Positions[i]);
                    }
                }
            }

            foreach (var inventoryViewModel in AllInventories)
            {
                if (inventoryViewModel.OwnerTypeId != "Hero")
                {
                    continue;
                }

                foreach (var inventoryGridViewModel in inventoryViewModel.AllInventoryGrids)
                {
                    if (inventoryGridViewModel.SwapItems(itemProxy, itemProxy2))
                    {
                        Debug.Log("SWAP");
                    }
                }
            }

            foreach (var inventory in gameState.Inventories)
            {
                foreach (var grid in inventory.Inventories)
                {
                    for (int i = 0; i < grid.Items.Count; i++)
                    {
                        Debug.Log(grid.Items[i].Id + " " + grid.Positions[i]);
                    }

                    for (int i = 0; i < grid.Grid.Value.Length; i++)
                    {
                        Debug.Log(grid.Grid.Value[i] + $" Grid in {inventory.OwnerTypeId} {inventory.OwnerId}");
                    }
                }
            }
        }

        public void HandleTestInputTab()
        {
            foreach (var inventoryViewModel in _inventoryService.AllInventories)
            {
                if (inventoryViewModel.OwnerTypeId != "Hero")
                {
                    continue;
                }

                foreach (var inventoryGridViewModel in inventoryViewModel.AllInventoryGrids)
                {
                    inventoryGridViewModel.AddItem(new ItemDataProxy(new ItemData()
                    {
                        CanRotate = false,
                        IsRotated = false,
                        Height = 1,
                        Width = 1,
                        Id = "Pin"
                    }));
                    // for (int i = 0; i < inventoryGridViewModel.Grid.Value.GetLength(0); i++)
                    // {
                    //     for (int j = 0; j < inventoryGridViewModel.Grid.Value.GetLength(1); j++)
                    //     {
                    //         Debug.Log(inventoryGridViewModel.Grid.Value[i,j] +
                    //                   $" Grid in {inventoryViewModel.OwnerTypeId} {inventoryViewModel.OwnerId}");
                    //     }
                    // }
                }
            }
            var inventories = _gameStateProvider.GameState._gameState.Inventories;

            foreach (var data in inventories)
            {
                if (data.OwnerTypeId != "Hero")
                {
                    continue;
                }
                foreach (var gridDataProxy in data.Inventories)
                {
                    for (int i = 0; i < gridDataProxy.Grid.Length; i++)
                    {
                        Debug.Log(gridDataProxy.Grid[i]);
                    }
                }
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