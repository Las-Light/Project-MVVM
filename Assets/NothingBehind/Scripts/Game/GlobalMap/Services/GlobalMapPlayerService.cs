using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.GlobalMap.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Utils;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GlobalMap.Services
{
    public class GlobalMapPlayerService
    {
        private readonly InputManager _inputManager;
        private readonly ReactiveProperty<Player> _player;
        private readonly SceneEnterParams _sceneEnterParams;
        private readonly ICommandProcessor _commandProcessor;

        public GlobalMapPlayerService(InputManager inputManager, 
            ReactiveProperty<Player> player,
            EquipmentService equipmentService,
            SceneEnterParams sceneEnterParams,
            ICommandProcessor commandProcessor)
        {
            _inputManager = inputManager;
            _player = player;
            _sceneEnterParams = sceneEnterParams;
            _commandProcessor = commandProcessor;
        }
        
        public CommandResult UpdatePlayerPosOnGlobalMap(Vector3 position, MapId currentMap)
        {
            var command = new CmdUpdatePlayerPosOnGlobalMap(position, currentMap);
            var result = _commandProcessor.Process(command);
            return result;
        }

        private CommandResult InitialPosOnMap()
        {
            var command = new CmdInitPlayerPosOnGlobalMap(_sceneEnterParams.TargetMapId);
            var result = _commandProcessor.Process(command);

            return result;
        }

        private void InitialPlayer()
        {
            InitialPosOnMap();
            CreatePlayerViewModel();
        }

        private void CreatePlayerViewModel()
        {
        }
    }
}