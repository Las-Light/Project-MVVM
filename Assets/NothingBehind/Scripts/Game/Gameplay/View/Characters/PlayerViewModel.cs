using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    public class PlayerViewModel
    {
        private readonly Player _player;
        private readonly PlayerService _playerService;
        private readonly PlayerMovementManager _playerMovementManager;
        private readonly PlayerTurnManager _playerTurnManager;
        private PlayerView _playerView;
        private CharacterController _playerCharacterController;
        private PlayerInput _playerInput;
        public IObservableCollection<PositionOnMapProxy> PositionOnMaps => _positionOnMaps;
        public ReadOnlyReactiveProperty<MapId> CurrentMap { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        private ObservableList<PositionOnMapProxy> _positionOnMaps { get; }

        public PlayerViewModel(
            Player player, 
            PlayerService playerService, 
            PlayerMovementManager playerMovementManager,
            PlayerTurnManager playerTurnManager)
        {
            CurrentMap = player.CurrentMap;
            Health = player.Health;
            _positionOnMaps = player.PositionOnMaps;
            
            _player = player;
            _playerService = playerService;
            _playerMovementManager = playerMovementManager;
            _playerTurnManager = playerTurnManager;
        }

        public void SetPlayerViewWithComponent(PlayerView playerView, Camera camera)
        {
            _playerView = playerView;
            _playerCharacterController = playerView.GetComponent<CharacterController>();
            _playerInput = playerView.GetComponent<PlayerInput>();
            _playerMovementManager.BindPlayerViewComponent(playerView, camera, _playerCharacterController);
            _playerTurnManager.BindPlayerViewComponent(playerView, camera, _playerInput);
        }

        public void Move()
        {
            _playerMovementManager.Move();
            _playerService.UpdatePlayerPosOnMap(_playerView.transform.position, CurrentMap.CurrentValue);
        }

        public void Look()
        {
            _playerTurnManager.LookGamepad();
            _playerTurnManager.LookMouse();
        }

        public bool InteractiveActionPressed()
        {
            return _playerMovementManager.InteractiveActionPressed();
        }
    }
}