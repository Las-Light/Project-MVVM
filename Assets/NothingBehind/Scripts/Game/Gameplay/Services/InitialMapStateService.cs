using System;
using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands;
using NothingBehind.Scripts.Game.Gameplay.View.Maps;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class InitialMapStateService
    {
        public readonly ObservableList<MapTransferViewModel> MapTransfers = new();
        private readonly Dictionary<MapId, MapTransferViewModel> _mapTransfers = new();

        private readonly IGameStateProvider _gameStateProvider;
        private readonly ICommandProcessor _commandProcessor;
        private readonly SceneEnterParams _sceneEnterParams;
        public Map LoadingMap { get; }


        public InitialMapStateService(
            IGameStateProvider gameStateProvider,
            ICommandProcessor commandProcessor,
            SceneEnterParams sceneEnterParams)
        {
            _gameStateProvider = gameStateProvider;
            _commandProcessor = commandProcessor;
            _sceneEnterParams = sceneEnterParams;

            LoadingMap = InitialMapState();

            InitialMapTransfers();
            // InitialEnemySpawn();
        }

        private Map InitialMapState()
        {
            var gameState = _gameStateProvider.GameState;
            var loadingMapId = _sceneEnterParams.TargetMapId;

            var loadingMap = gameState.Maps.FirstOrDefault(m => m.Id == loadingMapId);
            if (loadingMap == null)
            {
                // Создание состояния, если его еще нет через команду.
                var command = new CmdCreateMapState(loadingMapId);
                var success = _commandProcessor.Process(command);
                if (!success)
                {
                    throw new Exception($"Couldn't create map state with id: ${loadingMapId}");
                }
            }

            var currentMap = gameState.Maps.First(m => m.Id == loadingMapId);
            return currentMap;
        }

        private void InitialMapTransfers()
        {
            var mapTransfer = LoadingMap.MapTransfers;
            mapTransfer.ForEach(CreateMapTransferViewModel);
            mapTransfer.ObserveAdd().Subscribe(e => CreateMapTransferViewModel(e.Value));
            mapTransfer.ObserveRemove().Subscribe(e => RemoveMapTransferViewModel(e.Value));
        }

        private void CreateMapTransferViewModel(MapTransferData mapTransferData)
        {
            var mapTransferViewModel = new MapTransferViewModel(mapTransferData);
            _mapTransfers[mapTransferData.TargetMapId] = mapTransferViewModel;

            MapTransfers.Add(mapTransferViewModel);
        }

        private void RemoveMapTransferViewModel(MapTransferData mapTransferData)
        {
            if (_mapTransfers.TryGetValue(mapTransferData.TargetMapId, out var mapTransferViewModel))
            {
                MapTransfers.Remove(mapTransferViewModel);
                _mapTransfers.Remove(mapTransferData.TargetMapId);
            }
        }
    }
}