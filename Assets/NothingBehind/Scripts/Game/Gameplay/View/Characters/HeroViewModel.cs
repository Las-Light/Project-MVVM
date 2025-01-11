using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Entities.Hero;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    public class HeroViewModel
    {
        private readonly HeroProxy _heroProxy;
        private readonly HeroService _heroService;

        public IObservableCollection<PositionOnMapProxy> PositionOnMaps => _positionOnMaps;
        public ReadOnlyReactiveProperty<MapId> CurrentMap { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        private ObservableList<PositionOnMapProxy> _positionOnMaps { get; }

        public HeroViewModel(
            HeroProxy heroProxy, 
            HeroService heroService)
        {
            CurrentMap = heroProxy.CurrentMap;
            Health = heroProxy.Health;
            _positionOnMaps = heroProxy.PositionOnMaps;
            
            _heroProxy = heroProxy;
            _heroService = heroService;

        }
    }
}