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
        private readonly MoveHeroService _moveHeroService;
        private readonly LookHeroService _lookHeroService;
        private HeroBinder _heroView;
        private CharacterController _heroCharacterController;
        private PlayerInput _playerInput;
        public IObservableCollection<PositionOnMapProxy> PositionOnMaps => _positionOnMaps;
        public ReadOnlyReactiveProperty<PositionOnMapProxy> CurrentMap { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        private ObservableList<PositionOnMapProxy> _positionOnMaps { get; }

        public HeroViewModel(
            HeroProxy heroProxy, HeroService heroService, 
            MoveHeroService moveHeroService,
            LookHeroService lookHeroService)
        {
            CurrentMap = heroProxy.CurrentMap;
            Health = heroProxy.Health;
            _positionOnMaps = heroProxy.PositionOnMaps;
            
            _heroProxy = heroProxy;
            _heroService = heroService;
            _moveHeroService = moveHeroService;
            _lookHeroService = lookHeroService;
        }

        public void SetHeroViewWithComponent(HeroBinder heroView, Camera camera)
        {
            _heroView = heroView;
            _heroCharacterController = heroView.GetComponent<CharacterController>();
            _playerInput = heroView.GetComponent<PlayerInput>();
            _moveHeroService.BindHeroViewComponent(heroView, camera, _heroCharacterController);
            _lookHeroService.BindHeroViewComponent(heroView, camera, _playerInput);
        }

        public void Move()
        {
            _moveHeroService.Move();
            _heroService.UpdateHeroPosOnMap(_heroView.transform.position);
        }

        public void Look()
        {
            _lookHeroService.LookGamepad();
            _lookHeroService.LookMouse();
        }

        public bool InteractiveActionPressed()
        {
            return _moveHeroService.InteractiveActionPressed();
        }
    }
}