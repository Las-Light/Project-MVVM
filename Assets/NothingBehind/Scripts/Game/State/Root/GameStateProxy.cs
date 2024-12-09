using System.Linq;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Root
{
    public class GameStateProxy
    {
        public readonly GameState _gameState;
        public readonly ReactiveProperty<string> CurrentMapId = new();
        public ObservableList<Map> Maps { get; } = new();

        public GameStateProxy(GameState gameState)
        {
            _gameState = gameState;
            CurrentMapId.Value = gameState.CurrentMapId;
            InitMaps(gameState);

            CurrentMapId.Skip(1).Subscribe(newValue =>
            {
                Debug.Log("Changed CurrentMap = " + newValue);
                gameState.CurrentMapId = newValue;
            });
        }

        public int CreateEntityId()
        {
            return _gameState.CreateEntityId();
        }

        private void InitMaps(GameState gameState)
        {
            gameState.Maps.ForEach(mapOrigin => Maps.Add(new Map(mapOrigin)));

            Maps.ObserveAdd().Subscribe(e =>
            {
                var addedMap = e.Value;
                gameState.Maps.Add(addedMap.Origin);
            });

            Maps.ObserveRemove().Subscribe(e =>
            {
                var removedMap = e.Value;
                var removedMapState =
                    gameState.Maps.FirstOrDefault(c => c.Id == removedMap.Id);
                gameState.Maps.Remove(removedMapState);
            });
        }
    }
}