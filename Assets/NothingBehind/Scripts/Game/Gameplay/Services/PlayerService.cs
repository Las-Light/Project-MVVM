using System;
using NothingBehind.Scripts.Game.Gameplay.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
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

        private readonly PlayerMovementController _playerMovementController;
        private readonly LookPlayerController _lookPlayerController;
        private readonly Player _player;
        private readonly InventoryService _inventoryService;
        private readonly ArsenalService _arsenalService;
        private readonly GameplayInputManager _inputManager;
        private readonly ICommandProcessor _cmd;
        private readonly SceneEnterParams _sceneEnterParams;
        private readonly PlayerSettings _playerSettings;

        public PlayerService(InventoryService inventoryService,
            ArsenalService arsenalService,
            Player player,
            GameplayInputManager inputManager,
            ICommandProcessor cmd,
            SceneEnterParams sceneEnterParams,
            PlayerSettings playerSettings)
        {
            _inventoryService = inventoryService;
            _arsenalService = arsenalService;
            _player = player;
            _inputManager = inputManager;
            _cmd = cmd;
            _sceneEnterParams = sceneEnterParams;
            _playerSettings = playerSettings;

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
            CreatePlayerViewModel();
        }

        private void CreatePlayerViewModel()
        {
            if (!_arsenalService.ArsenalMap.TryGetValue(_player.Id, out var arsenalViewModel))
            {
                throw new Exception($"ArsenalViewModel for owner with Id {_player.Id} not found");
            }
            var viewModel = new PlayerViewModel(_player,this, _inputManager, arsenalViewModel, _playerSettings);

            PlayerViewModel.Value = viewModel;
        }
    }
}