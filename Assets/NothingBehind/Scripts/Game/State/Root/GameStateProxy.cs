using System.Linq;
using NothingBehind.Scripts.Game.State.Entities.Hero;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Root
{
    public class GameStateProxy
    {
        //TODO: сделать поле приватным
        public readonly GameState _gameState;
        public readonly ReactiveProperty<MapId> CurrentMapId = new();
        public ReactiveProperty<Player> Hero { get; } = new();
        public ObservableList<Map> Maps { get; } = new();
        public ObservableList<Resource> Resources { get; } = new();
        public ObservableList<Inventory.Inventory> Inventories { get; } = new();

        public GameStateProxy(GameState gameState)
        {
            _gameState = gameState;
            CurrentMapId.Value = gameState.CurrentMapId;

            InitMaps(gameState);
            InitResources(gameState);
            InitInventories(gameState);
            InitHero(gameState);

            CurrentMapId.Skip(1).Subscribe(newValue => gameState.CurrentMapId = newValue);
        }

        public int CreateEntityId()
        {
            return _gameState.CreateEntityId();
        }

        public int CreateItemId()
        {
            return _gameState.CreateItemId();
        }

        private void InitHero(GameState gameState)
        {
            Hero.Value = new Player(gameState.playerData);
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

        private void InitInventories(GameState gameState)
        {
            gameState.Inventories.ForEach(inventoryOrigin => Inventories.Add(new Inventory.Inventory(inventoryOrigin)));

            Inventories.ObserveAdd().Subscribe(e =>
            {
                var addedInventory = e.Value;
                gameState.Inventories.Add(addedInventory.Origin);
            });

            Inventories.ObserveRemove().Subscribe(e =>
            {
                var removedInventory = e.Value;
                var removedInventoryData = gameState.Inventories.FirstOrDefault(c =>
                    c.OwnerId == removedInventory.OwnerId);
                gameState.Inventories.Remove(removedInventoryData);
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