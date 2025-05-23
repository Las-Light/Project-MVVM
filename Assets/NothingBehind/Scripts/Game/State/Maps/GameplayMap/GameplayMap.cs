using System.Linq;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Maps.GameplayMap
{
    public class GameplayMap
    {
        public MapId Id => Origin.Id;
        public string SceneName => Origin.SceneName;
        public ObservableList<Character> Characters { get; } = new();
        public ObservableList<Storage> Storages { get; } = new();
        public ObservableList<MapTransferData> MapTransfers { get; } = new();
        public ObservableList<EnemySpawn> EnemySpawns { get; } = new();
        public GameplayMapData Origin { get; }

        public GameplayMap(GameplayMapData gameplayMapState)
        {
            Origin = gameplayMapState;
            gameplayMapState.Characters.ForEach(characterOrigin =>
                Characters.Add(new Character(characterOrigin)));
            gameplayMapState.Storages.ForEach(storageOrigin => 
                Storages.Add(new Storage(storageOrigin)));
            gameplayMapState.MapTransfers.ForEach(mapTransferData =>
                MapTransfers.Add(mapTransferData));
            gameplayMapState.EnemySpawns.ForEach(enemySpawnData =>
                EnemySpawns.Add(new EnemySpawn(enemySpawnData)));

            Characters.ObserveAdd().Subscribe(e =>
            {
                var addedCharacterEntity = e.Value;
                gameplayMapState.Characters.Add(addedCharacterEntity.Origin);
            });

            Characters.ObserveRemove().Subscribe(e =>
            {
                var removedCharacter = e.Value;
                var removedCharacterData =
                    gameplayMapState.Characters.FirstOrDefault(c => c.UniqueId == removedCharacter.Id);
                gameplayMapState.Characters.Remove(removedCharacterData);
            });

            Storages.ObserveAdd().Subscribe(e =>
            {
                var addedStorage = e.Value;
                gameplayMapState.Storages.Add(addedStorage.Origin);
            });
            Storages.ObserveRemove().Subscribe(e =>
            {
                var removedStorage = e.Value;
                var removedStorageData =
                    gameplayMapState.Storages.FirstOrDefault(c => c.UniqueId == removedStorage.Id);
                gameplayMapState.Storages.Remove(removedStorageData);
            });
        }
    }
}