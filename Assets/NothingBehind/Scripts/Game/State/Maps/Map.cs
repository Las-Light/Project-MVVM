using System.Linq;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Maps
{
    public class Map
    {
        public MapId Id => Origin.Id;
        public string SceneName => Origin.SceneName;
        public ObservableList<Character> Characters { get; } = new();
        public ObservableList<MapTransferData> MapTransfers { get; } = new();
        public ObservableList<EnemySpawn> EnemySpawns { get; } = new();
        public MapData Origin { get; }

        public Map(MapData mapState)
        {
            Origin = mapState;
            mapState.Characters.ForEach(characterOrigin =>
                Characters.Add(new Character(characterOrigin)));
            mapState.MapTransfers.ForEach(mapTransferData =>
                MapTransfers.Add(new MapTransferData(
                    mapTransferData.TargetMapId,
                    mapTransferData.Position)));
            mapState.EnemySpawns.ForEach(enemySpawnData =>
                EnemySpawns.Add(new EnemySpawn(enemySpawnData)));

            Characters.ObserveAdd().Subscribe(e =>
            {
                var addedCharacterEntity = e.Value;
                mapState.Characters.Add(addedCharacterEntity.Origin);
            });

            Characters.ObserveRemove().Subscribe(e =>
            {
                var removedCharacterEntityProxy = e.Value;
                var removedCharacterEntity =
                    mapState.Characters.FirstOrDefault(c => c.UniqueId == removedCharacterEntityProxy.Id);
                mapState.Characters.Remove(removedCharacterEntity);
            });
        }
    }
}