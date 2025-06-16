using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Camera;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Inventories;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Player;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Transfers;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.GlobalMap.Root.View
{
    public class WorldGlobalMapRootViewModel
    {
        public readonly ObservableList<MapTransferViewModel> AllMapTransfers;
        public Dictionary<int,InventoryViewModel> InventoriesMap { get; }
        public ReactiveProperty<PlayerViewModel> Player { get; }
        public ReadOnlyReactiveProperty<CameraViewModel> Camera { get; }



        public WorldGlobalMapRootViewModel(
            ISettingsProvider settingsProvider,
            IGameStateProvider gameStateProvider,
            PlayerService playerService,
            ResourcesService resourcesService,
            MapTransferService mapService,
            InputManager inputManager,
            CameraService cameraService,
            InventoryService inventoryService)
        {
            AllMapTransfers = mapService.AllMapTransferViewModels;
            InventoriesMap = inventoryService.InventoryMap;
            Player = playerService.PlayerViewModel;
            Camera = cameraService.CameraViewModel;
            
            var gameState = gameStateProvider.GameState;
            
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
        }
    }
}