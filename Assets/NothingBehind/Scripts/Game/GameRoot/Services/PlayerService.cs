using NothingBehind.Scripts.Game.GameRoot.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Player;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Player;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Utils;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.Services
{
    public class PlayerService
    {
        public readonly ReactiveProperty<PlayerViewModel> PlayerViewModel = new();

        private readonly InputManager.InputManager _inputManager;
        private readonly ICommandProcessor _cmd;
        private readonly SceneEnterParams _sceneEnterParams;
        private readonly PlayerSettings _playerSettings;

        public PlayerService(
            PlayerEntity playerEntity,
            InputManager.InputManager inputManager,
            ICommandProcessor cmd,
            PlayerSettings playerSettings)
        {
            _inputManager = inputManager;
            _cmd = cmd;
            _playerSettings = playerSettings;

            InitialPlayer(playerEntity);
        }

        public void UpdatePlayerService(PlayerEntity playerEntity)
        {
            InitialPlayer(playerEntity);
        }

        public CommandResult UpdatePlayerPosOnMap(Vector3 position, MapId currentMap)
        {
            var command = new CmdUpdatePlayerPosOnMap(position, currentMap);
            var result = _cmd.Process(command);
            return result;
        }

        public CommandResult InitialPosOnMap()
        {
            var command = new CmdInitPlayerPosOnMap();
            var result = _cmd.Process(command);

            return result;
        }

        private void InitialPlayer(PlayerEntity playerEntity)
        {
            InitialPosOnMap();
            CreatePlayerViewModel(playerEntity);
        }

        private void CreatePlayerViewModel(PlayerEntity playerEntity)
        {
            var viewModel = new PlayerViewModel(
                playerEntity,
                this,
                _inputManager,
                _playerSettings);

            PlayerViewModel.Value = viewModel;
        }
    }
}