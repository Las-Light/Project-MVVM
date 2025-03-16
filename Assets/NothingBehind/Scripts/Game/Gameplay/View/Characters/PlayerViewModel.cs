using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.Logic.Hero;
using NothingBehind.Scripts.Game.Gameplay.Services.Hero;
using NothingBehind.Scripts.Game.State.Entities.Hero;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    public class PlayerViewModel
    {
        private readonly Player _player;
        private readonly HeroService _heroService;
        private readonly HeroMovementManager _heroMovementManager;
        private readonly HeroTurnManager _heroTurnManager;
        private PlayerView _playerView;
        private CharacterController _playerCharacterController;
        private PlayerInput _playerInput;
        public IObservableCollection<PositionOnMapProxy> PositionOnMaps => _positionOnMaps;
        public ReadOnlyReactiveProperty<PositionOnMapProxy> CurrentMap { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        private ObservableList<PositionOnMapProxy> _positionOnMaps { get; }

        public PlayerViewModel(
            Player player, 
            HeroService heroService, 
            HeroMovementManager heroMovementManager,
            HeroTurnManager heroTurnManager)
        {
            CurrentMap = player.CurrentMap;
            Health = player.Health;
            _positionOnMaps = player.PositionOnMaps;
            
            _player = player;
            _heroService = heroService;
            _heroMovementManager = heroMovementManager;
            _heroTurnManager = heroTurnManager;
        }

        public void SetHeroViewWithComponent(PlayerView playerView, Camera camera)
        {
            _playerView = playerView;
            _playerCharacterController = playerView.GetComponent<CharacterController>();
            _playerInput = playerView.GetComponent<PlayerInput>();
            _heroMovementManager.BindHeroViewComponent(playerView, camera, _playerCharacterController);
            _heroTurnManager.BindHeroViewComponent(playerView, camera, _playerInput);
        }

        public void Move()
        {
            _heroMovementManager.Move();
            _heroService.UpdateHeroPosOnMap(_playerView.transform.position);
        }

        public void Look()
        {
            _heroTurnManager.LookGamepad();
            _heroTurnManager.LookMouse();
        }

        public bool InteractiveActionPressed()
        {
            return _heroMovementManager.InteractiveActionPressed();
        }
    }
}