using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Camera;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Player;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Transfers;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.Camera;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.Maps;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.Player;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.UI;
using NothingBehind.Scripts.Game.State.Maps;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GlobalMap.Root.View
{
    public class WorldGlobalMapRootView : MonoBehaviour
    {
        private WorldGlobalMapRootViewModel _viewModel;
        private GMPlayerView _playerView;
        private GMCameraView _camera;
        private readonly Dictionary<MapId, GlobalMapTransferView> _createMapTransfersMap = new();

        public void Bind(WorldGlobalMapRootViewModel viewModel,
            GlobalMapUIManager globalMapUIManager,
            Subject<GlobalMapExitParams> exitSceneRequest)
        {
            _viewModel = viewModel;
            CreatePlayer(viewModel.Player.Value, globalMapUIManager);
            //CreateCamera(viewModel.Camera.CurrentValue);
            foreach (var mapTransferViewModel in viewModel.AllMapTransfers)
                CreateMapTransfer(mapTransferViewModel, exitSceneRequest);
        }

        private void CreatePlayer(PlayerViewModel playerViewModel,
            GlobalMapUIManager globalMapUIManager)
        {
            var prefabHeroPath = "Prefabs/GlobalMap/World/Entities/Player/PlayerView";
            var heroPrefab = Resources.Load<GMPlayerView>(prefabHeroPath);

            if (!_viewModel.InventoriesMap.TryGetValue(_viewModel.Player.CurrentValue.Id, out var inventoryViewModel))
                throw new Exception(
                    $"InventoryViewModel for owner with Id {_viewModel.Player.CurrentValue.Id} not found");

            var playerView = Instantiate(heroPrefab);
            playerView.Bind(playerViewModel, inventoryViewModel, globalMapUIManager);
            _playerView = playerView;
        }

        private void CreateCamera(CameraViewModel cameraViewModel)
        {
            var prefabCameraPath = "Prefabs/GlobalMap/World/Camera/VirtualCamera";
            var cameraPrefab = Resources.Load<GMCameraView>(prefabCameraPath);

            var cameraView = Instantiate(cameraPrefab);
            cameraView.Bind(cameraViewModel);
            _camera = cameraView;
        }

        private void CreateMapTransfer(MapTransferViewModel mapTransferViewModel,
            Subject<GlobalMapExitParams> exitSceneSignal)
        {
            var transferId = mapTransferViewModel.MapId;
            var prefabMapTransferPath = "Prefabs/GlobalMap/World/MapTransfers/MapTransfer";
            var mapTransferPrefab = Resources.Load<GlobalMapTransferView>(prefabMapTransferPath);

            var createdMapTransfer = Instantiate(mapTransferPrefab);
            createdMapTransfer.Bind(exitSceneSignal, mapTransferViewModel);

            _createMapTransfersMap[transferId] = createdMapTransfer;
        }
    }
}