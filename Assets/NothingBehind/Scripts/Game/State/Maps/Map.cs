using System.Linq;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Maps
{
    public class Map
    {
        public MapId Id => Origin.Id;
        public string SceneName => Origin.SceneName;
        public ObservableList<CharacterEntityProxy> Characters { get; } = new();
        public ObservableList<MapTransferData> MapTransfers { get; } = new();
        public MapState Origin { get; }

        public Map(MapState mapState)
        {
            Origin = mapState;
            mapState.Characters.ForEach(characterOrigin => Characters.Add(new CharacterEntityProxy(characterOrigin)));
            mapState.MapTransfers.ForEach(mapTransfer =>
                MapTransfers.Add(new MapTransferData(
                    mapTransfer.MapId,
                    mapTransfer.Position)));
            Characters.ObserveAdd().Subscribe(e =>
            {
                var addedCharacterEntity = e.Value;
                mapState.Characters.Add(addedCharacterEntity.Origin);
            });

            Characters.ObserveRemove().Subscribe(e =>
            {
                var removedCharacterEntityProxy = e.Value;
                var removedCharacterEntity =
                    mapState.Characters.FirstOrDefault(c => c.Id == removedCharacterEntityProxy.Id);
                mapState.Characters.Remove(removedCharacterEntity);
            });
        }
    }
}