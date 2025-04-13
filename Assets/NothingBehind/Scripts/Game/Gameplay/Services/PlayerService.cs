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

        private readonly MovementController _movementController;
        private readonly TurnController _turnController;
        private readonly Player _player;
        private readonly ICommandProcessor _cmd;
        private readonly SceneEnterParams _sceneEnterParams;

        public PlayerService(InventoryService inventoryService,
            ArsenalService arsenalService,
            Player player,
            GameplayInputManager inputManager,
            ICommandProcessor cmd,
            SceneEnterParams sceneEnterParams,
            PlayerSettings playerSettings)
        {
            _cmd = cmd;
            _sceneEnterParams = sceneEnterParams;

            InitialPlayer(player, arsenalService, inputManager, playerSettings);
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

        private void InitialPlayer(Player player, ArsenalService arsenalService, GameplayInputManager inputManager,
            PlayerSettings playerSettings)
        {
            InitialPosOnMap();
            CreatePlayerViewModel(player, arsenalService, inputManager, playerSettings);
        }

        private void CreatePlayerViewModel(Player player, ArsenalService arsenalService,
            GameplayInputManager inputManager, PlayerSettings playerSettings)
        {
            if (!arsenalService.ArsenalMap.TryGetValue(player.Id, out var arsenalViewModel))
            {
                throw new Exception($"ArsenalViewModel for owner with Id {player.Id} not found");
            }
            var viewModel = new PlayerViewModel(player,this, inputManager, arsenalViewModel, playerSettings);

            PlayerViewModel.Value = viewModel;
        }
    }
}