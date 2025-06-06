using NothingBehind.Scripts.Game.BattleGameplay.Root.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Player;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
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
            Player player,
            InputManager.InputManager inputManager,
            ICommandProcessor cmd,
            PlayerSettings playerSettings)
        {
            _inputManager = inputManager;
            _cmd = cmd;
            _playerSettings = playerSettings;

            InitialPlayer(player);
        }

        public void UpdatePlayerService(Player player)
        {
            InitialPlayer(player);
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

        private void InitialPlayer(Player player)
        {
            InitialPosOnMap();
            CreatePlayerViewModel(player);
        }

        private void CreatePlayerViewModel(Player player)
        {
            var viewModel = new PlayerViewModel(
                player,
                this,
                _inputManager,
                _playerSettings);

            PlayerViewModel.Value = viewModel;
        }
    }
}