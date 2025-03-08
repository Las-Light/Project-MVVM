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
    public class HeroViewModel
    {
        private readonly HeroProxy _heroProxy;
        private readonly HeroService _heroService;
        private readonly HeroMovementManager _heroMovementManager;
        private readonly HeroTurnManager _heroTurnManager;
        private HeroBinder _heroView;
        private CharacterController _heroCharacterController;
        private PlayerInput _playerInput;
        public IObservableCollection<PositionOnMapProxy> PositionOnMaps => _positionOnMaps;
        public ReadOnlyReactiveProperty<PositionOnMapProxy> CurrentMap { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        private ObservableList<PositionOnMapProxy> _positionOnMaps { get; }

        public HeroViewModel(
            HeroProxy heroProxy, 
            HeroService heroService, 
            HeroMovementManager heroMovementManager,
            HeroTurnManager heroTurnManager)
        {
            CurrentMap = heroProxy.CurrentMap;
            Health = heroProxy.Health;
            _positionOnMaps = heroProxy.PositionOnMaps;
            
            _heroProxy = heroProxy;
            _heroService = heroService;
            _heroMovementManager = heroMovementManager;
            _heroTurnManager = heroTurnManager;
        }

        public void SetHeroViewWithComponent(HeroBinder heroView, Camera camera)
        {
            _heroView = heroView;
            _heroCharacterController = heroView.GetComponent<CharacterController>();
            _playerInput = heroView.GetComponent<PlayerInput>();
            _heroMovementManager.BindHeroViewComponent(heroView, camera, _heroCharacterController);
            _heroTurnManager.BindHeroViewComponent(heroView, camera, _playerInput);
        }

        public void Move()
        {
            _heroMovementManager.Move();
            _heroService.UpdateHeroPosOnMap(_heroView.transform.position);
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