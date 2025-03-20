using System.Linq;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Root
{
    public class GameStateProxy
    {
        //TODO: сделать поле приватным
        public readonly GameState _gameState;
        public readonly ReactiveProperty<MapId> CurrentMapId = new();
        public ReactiveProperty<Player> Player { get; }
        public ObservableList<Map> Maps { get; } = new();
        public ObservableList<Resource> Resources { get; } = new();
        public ObservableList<Inventory> Inventories { get; } = new();
        public ObservableList<Equipment> Equipments { get; } = new();

        public GameStateProxy(GameState gameState)
        {
            _gameState = gameState;
            CurrentMapId.Value = gameState.CurrentMapId;

            InitMaps(gameState);
            InitResources(gameState);
            InitInventories(gameState);
            InitEquipments(gameState);
            Player = new ReactiveProperty<Player>(new Player(gameState.PlayerData));
            Player.Subscribe(player => gameState.PlayerData = player.Origin);

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
        
        public int CreateGridId()
        {
            return _gameState.CreateGridId();
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
            gameState.Inventories.ForEach(inventoryOrigin => Inventories.Add(new Inventory(inventoryOrigin)));

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
        
        private void InitEquipments(GameState gameState)
        {
            gameState.Equipments.ForEach(equipmentData => Equipments.Add(new Equipment(equipmentData)));

            Equipments.ObserveAdd().Subscribe(e =>
            {
                var addedEquipment = e.Value;
                gameState.Equipments.Add(addedEquipment.Origin);
            });

            Equipments.ObserveRemove().Subscribe(e =>
            {
                var removedEquipment = e.Value;
                var removedEquipmentData = gameState.Equipments.FirstOrDefault(c =>
                    c.OwnerId == removedEquipment.OwnerId);
                gameState.Equipments.Remove(removedEquipmentData);
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