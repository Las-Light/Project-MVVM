using System.Linq;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Entities.Hero
{
    public class Player
    {
        public int Id { get; }
        public string TypeId { get; }
        public PlayerData Origin { get; }
        public ReactiveProperty<float> Health { get; }
        public ReactiveProperty<PositionOnMapProxy> CurrentMap { get; }

        public ObservableList<PositionOnMapProxy> PositionOnMaps { get; } = new();

        public Player(PlayerData playerData)
        {
            Origin = playerData;
            Id = playerData.UniqueId;
            TypeId = playerData.TypeId;
            
            InitPosOnMaps(playerData);

            CurrentMap = new ReactiveProperty<PositionOnMapProxy>(new PositionOnMapProxy(playerData.CurrentMap));
            Health = new ReactiveProperty<float>(playerData.Health);
            CurrentMap.Skip(1).Subscribe(value => playerData.CurrentMap = value.Origin);
            Health.Skip(1).Subscribe(value => playerData.Health = value);
        }

        private void InitPosOnMaps(PlayerData playerData)
        {
            playerData.PositionOnMaps.ForEach(positionOnMap => PositionOnMaps.Add(new PositionOnMapProxy(positionOnMap)));
            
            PositionOnMaps.ObserveAdd().Subscribe(e =>
            {
                var addedPosOnMap = e.Value;
                playerData.PositionOnMaps.Add(addedPosOnMap.Origin);
            });
            PositionOnMaps.ObserveRemove().Subscribe(e =>
            {
                var removedPosOnMapProxy = e.Value;
                var removedPosOnMap =
                    playerData.PositionOnMaps.FirstOrDefault(c => c.MapId == removedPosOnMapProxy.MapId);
                playerData.PositionOnMaps.Remove(removedPosOnMap);
            });
        }
    }
}