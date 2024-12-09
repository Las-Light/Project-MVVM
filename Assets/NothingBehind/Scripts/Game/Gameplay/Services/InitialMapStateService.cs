using System;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class InitialMapStateService
    {
        private readonly IGameStateProvider _gameStateProvider;
        private readonly ICommandProcessor _commandProcessor;
        private readonly SceneEnterParams _sceneEnterParams;

        public InitialMapStateService(IGameStateProvider gameStateProvider, ICommandProcessor commandProcessor,
            SceneEnterParams sceneEnterParams)
        {
            _gameStateProvider = gameStateProvider;
            _commandProcessor = commandProcessor;
            _sceneEnterParams = sceneEnterParams;

            InitialMapState();
        }

        private void InitialMapState()
        {
            var gameState = _gameStateProvider.GameState;
            var loadingMapId = _sceneEnterParams.MapId;

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
        }
    }
}