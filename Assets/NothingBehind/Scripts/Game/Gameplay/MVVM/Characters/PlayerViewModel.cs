using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Characters
{
    public class PlayerViewModel
    {
        public int Id { get; }
        public IObservableCollection<PositionOnMap> PositionOnMaps => _positionOnMaps;
        public ReadOnlyReactiveProperty<MapId> CurrentMapId { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        public ReadOnlyReactiveProperty<Vector3> Position { get; }
        
        private ObservableList<PositionOnMap> _positionOnMaps { get; }
        
        private readonly Player _player;
        private readonly PlayerService _playerService;
        private readonly PlayerMovementManager _playerMovementManager;
        private readonly PlayerTurnManager _playerTurnManager;
        private PlayerView _playerView;
        private CharacterController _playerCharacterController;
        private PlayerInput _playerInput;


        public PlayerViewModel(
            Player player, 
            PlayerService playerService, 
            PlayerMovementManager playerMovementManager,
            PlayerTurnManager playerTurnManager)
        {
            Id = player.Id;
            CurrentMapId = player.CurrentMapId;
            Health = player.Health;
            _positionOnMaps = player.PositionOnMaps;
            
            _player = player;
            _playerService = playerService;
            _playerMovementManager = playerMovementManager;
            _playerTurnManager = playerTurnManager;

            var currentPosOnMap = _positionOnMaps.FirstOrDefault(map => map.MapId == CurrentMapId.CurrentValue);
            if (currentPosOnMap != null) Position = currentPosOnMap.Position;
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
            _playerService.UpdatePlayerPosOnMap(_playerView.transform.position, CurrentMapId.CurrentValue);
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