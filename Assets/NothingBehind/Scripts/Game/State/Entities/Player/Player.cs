using System.Linq;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Entities.Player
{
    public class Player
    {
        public int Id { get; }
        public EntityType EntityType { get; }
        public PlayerData Origin { get; }
        public ReactiveProperty<float> Health { get; }
        public ReactiveProperty<MapId> CurrentMapId { get; }
        public ObservableList<PositionOnMap> PositionOnMaps { get; } = new();

        public Player(PlayerData playerData)
        {
            Origin = playerData;
            Id = playerData.UniqueId;
            EntityType = playerData.EntityType;
            
            InitPosOnMaps(playerData);

            CurrentMapId = new ReactiveProperty<MapId>(playerData.CurrentMapId);
            Health = new ReactiveProperty<float>(playerData.Health);
            CurrentMapId.Skip(1).Subscribe(value => playerData.CurrentMapId = value);
            Health.Skip(1).Subscribe(value => playerData.Health = value);
        }

        private void InitPosOnMaps(PlayerData playerData)
        {
            playerData.PositionOnMaps.ForEach(positionOnMap => PositionOnMaps.Add(new PositionOnMap(positionOnMap)));
            
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