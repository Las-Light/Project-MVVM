using System.Linq;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Entities.Hero
{
    public class HeroProxy
    {
        public int Id { get; }
        public string TypeId { get; }
        public Hero Origin { get; }
        public ReactiveProperty<float> Health { get; }
        public ReactiveProperty<PositionOnMapProxy> CurrentMap { get; }

        public ObservableList<PositionOnMapProxy> PositionOnMaps { get; } = new();

        public HeroProxy(Hero hero)
        {
            Origin = hero;
            Id = hero.Id;
            TypeId = hero.TypeId;
            
            InitPosOnMaps(hero);

            CurrentMap = new ReactiveProperty<PositionOnMapProxy>(new PositionOnMapProxy(hero.CurrentMap));
            Health = new ReactiveProperty<float>(hero.Health);
            CurrentMap.Skip(1).Subscribe(value => hero.CurrentMap = value.Origin);
            Health.Skip(1).Subscribe(value => hero.Health = value);
        }

        private void InitPosOnMaps(Hero hero)
        {
            hero.PositionOnMaps.ForEach(positionOnMap => PositionOnMaps.Add(new PositionOnMapProxy(positionOnMap)));
            
            PositionOnMaps.ObserveAdd().Subscribe(e =>
            {
                var addedPosOnMap = e.Value;
                hero.PositionOnMaps.Add(addedPosOnMap.Origin);
            });
            PositionOnMaps.ObserveRemove().Subscribe(e =>
            {
                var removedPosOnMapProxy = e.Value;
                var removedPosOnMap =
                    hero.PositionOnMaps.FirstOrDefault(c => c.MapId == removedPosOnMapProxy.MapId);
                hero.PositionOnMaps.Remove(removedPosOnMap);
            });
        }
    }
}