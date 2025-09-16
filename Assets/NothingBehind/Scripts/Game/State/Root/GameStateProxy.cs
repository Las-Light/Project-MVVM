using System.Linq;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.GameplayMaps;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Root
{
    public class GameStateProxy
    {
        //TODO: сделать поле приватным
        public readonly GameState GameState;
        public readonly ReactiveProperty<MapId> CurrentMapId = new();
        public ReactiveProperty<PlayerEntity> Player { get; }
        public ObservableList<Map> Maps { get; } = new();
        public ObservableList<Resource> Resources { get; } = new();

        public GameStateProxy(GameState gameState)
        {
            GameState = gameState;
            CurrentMapId.Value = gameState.CurrentMapId;

            InitMaps(gameState);
            InitResources(gameState);
            Player = new ReactiveProperty<PlayerEntity>(new PlayerEntity(gameState.PlayerEntityData));
            Player.Subscribe(player => gameState.PlayerEntityData = player.Origin as PlayerEntityData);

            CurrentMapId.Skip(1).Subscribe(newValue => gameState.CurrentMapId = newValue);
        }

        public int CreateEntityId()
        {
            return GameState.CreateEntityId();
        }

        public int CreateItemId()
        {
            return GameState.CreateItemId();
        }

        public int CreateGridId()
        {
            return GameState.CreateGridId();
        }

        private void InitMaps(GameState gameState)
        {
            gameState.Maps.ForEach(mapOrigin => Maps.Add(MapFactory.CreateMap(mapOrigin)));

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

        private void InitResources(GameState gameState)
        {
            gameState.Resources.ForEach(resourceOrigin => Resources.Add(new Resource(resourceOrigin)));

            Resources.ObserveAdd().Subscribe(e =>
            {
                var addedResource = e.Value;
                gameState.Resources.Add(addedResource.Origin);
            });

            Resources.ObserveRemove().Subscribe(e =>
            {
                var removedResource = e.Value;
                var removedResourceData =
                    gameState.Resources.FirstOrDefault(c => c.ResourceType == removedResource.ResourceType);
                gameState.Resources.Remove(removedResourceData);
            });
        }
    }
}