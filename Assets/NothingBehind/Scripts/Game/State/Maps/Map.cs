using System.Linq;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Maps
{
    public class Map
    {
        public MapType MapType => Origin.MapType;
        public MapId Id => Origin.Id;
        public string SceneName => Origin.SceneName;
        public ObservableList<Entity> Entities { get; } = new();
        public ObservableList<MapTransferData> MapTransfers { get; } = new();
        public ObservableList<EnemySpawn> EnemySpawns { get; } = new();
        public MapData Origin { get; }

        public Map(MapData mapData)
        {
            Origin = mapData;
            mapData.Entities.ForEach(entityData =>
                Entities.Add(EntitiesFactory.CreateEntity(entityData)));
            mapData.MapTransfers.ForEach(mapTransferData =>
                MapTransfers.Add(mapTransferData));
            mapData.EnemySpawns.ForEach(enemySpawnData =>
                EnemySpawns.Add(new EnemySpawn(enemySpawnData)));

            Entities.ObserveAdd().Subscribe(e =>
            {
                var addedEntity = e.Value;
                mapData.Entities.Add(addedEntity.Origin);
            });
            
            Entities.ObserveRemove().Subscribe(e =>
            {
                var removedEntity = e.Value;
                var removedEntityData =
                    mapData.Entities.FirstOrDefault(c => c.UniqueId == removedEntity.UniqueId);
                mapData.Entities.Remove(removedEntityData);
            });
        }
    }
}