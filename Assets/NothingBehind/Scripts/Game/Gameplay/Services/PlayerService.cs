using NothingBehind.Scripts.Game.Gameplay.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Utils;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class PlayerService
    {
        public readonly ReactiveProperty<PlayerViewModel> PlayerViewModel = new();

        private readonly PlayerMovementManager _playerMovementManager;
        private readonly PlayerTurnManager _playerTurnManager;
        private readonly Player _player;
        private readonly ICommandProcessor _cmd;
        private readonly SceneEnterParams _sceneEnterParams;

        public PlayerService(InventoryService inventoryService,
            PlayerMovementManager playerMovementManager,
            PlayerTurnManager playerTurnManager,
            Player player,
            ICommandProcessor cmd,
            SceneEnterParams sceneEnterParams)
        {
            _playerMovementManager = playerMovementManager;
            _playerTurnManager = playerTurnManager;
            _player = player;
            _cmd = cmd;
            _sceneEnterParams = sceneEnterParams;

            InitialPlayer();
        }

        public CommandResult UpdatePlayerPosOnMap(Vector3 position, MapId currentMap)
        {
            var command = new CmdUpdatePlayerPosOnMap(position, currentMap);
            var result = _cmd.Process(command);
            return result;
        }

        private CommandResult InitialPosOnMap()
        {
            var command = new CmdInitPlayerPosOnMap(_sceneEnterParams.TargetMapId);
            var result = _cmd.Process(command);

            return result;
        }

        private void InitialPlayer()
        {
            InitialPosOnMap();
            var hero = _player;
            CreatePlayerViewModel(hero);
        }

        private void CreatePlayerViewModel(Player player)
        {
            var viewModel = new PlayerViewModel(player,this, _playerMovementManager, _playerTurnManager);

            PlayerViewModel.Value = viewModel;
        }
    }
}