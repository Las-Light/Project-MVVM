using System.Linq;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Root
{
    public class GameStateProxy
    {
        private readonly GameState _gameState;
        public ObservableList<CharacterEntityProxy> Characters { get; } = new();

        public GameStateProxy(GameState gameState)
        {
            _gameState = gameState;
            gameState.Characters.ForEach(characterOrigin => Characters.Add(new CharacterEntityProxy(characterOrigin)));
            Characters.ObserveAdd().Subscribe(e =>
            {
                var addedCharacterEntity = e.Value;
                gameState.Characters.Add(addedCharacterEntity.Origin);
            });

            Characters.ObserveRemove().Subscribe(e =>
            {
                var removedCharacterEntityProxy = e.Value;
                var removedCharacterEntity =
                    gameState.Characters.FirstOrDefault(c => c.Id == removedCharacterEntityProxy.Id);
                gameState.Characters.Remove(removedCharacterEntity);
            });
        }

        public int GetEntityID()
        {
            return _gameState.GlobalEntityId++;
        }
    }
}