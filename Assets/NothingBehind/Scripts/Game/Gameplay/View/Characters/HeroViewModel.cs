using NothingBehind.Scripts.Game.Gameplay.Services.Hero;
using NothingBehind.Scripts.Game.State.Entities.Hero;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    public class HeroViewModel
    {
        private readonly HeroProxy _heroProxy;
        private readonly HeroService _heroService;
        private readonly MoveHeroService _moveHeroService;
        public HeroBinder HeroView { get; set; }
        public IObservableCollection<PositionOnMapProxy> PositionOnMaps => _positionOnMaps;
        public ReadOnlyReactiveProperty<PositionOnMapProxy> CurrentMap { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        private ObservableList<PositionOnMapProxy> _positionOnMaps { get; }

        public HeroViewModel(
            HeroProxy heroProxy, HeroService heroService, 
            MoveHeroService moveHeroService)
        {
            CurrentMap = heroProxy.CurrentMap;
            Health = heroProxy.Health;
            _positionOnMaps = heroProxy.PositionOnMaps;
            
            _heroProxy = heroProxy;
            _heroService = heroService;
            _moveHeroService = moveHeroService;

        }

        public void SetHeroView(HeroBinder heroView)
        {
            HeroView = heroView;
        }

        public void Move()
        {
            HeroView.CharacterController.Move(_moveHeroService.Move());
            _heroService.UpdateHeroPosOnMap(HeroView.transform.position);
        }
    }
}