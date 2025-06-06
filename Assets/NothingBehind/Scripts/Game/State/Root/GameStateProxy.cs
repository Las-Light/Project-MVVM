using System.Linq;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.GameplayMap;
using NothingBehind.Scripts.Game.State.Weapons;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Root
{
    public class GameStateProxy
    {
        //TODO: сделать поле приватным
        public readonly GameState GameState;
        public readonly ReactiveProperty<MapId> CurrentMapId = new();
        public ReactiveProperty<Player> Player { get; }
        public ReactiveProperty<Maps.GlobalMaps.GlobalMap> GlobalMap { get; }
        public ObservableList<GameplayMap> Maps { get; } = new();
        public ObservableList<Resource> Resources { get; } = new();
        public ObservableList<Inventory> Inventories { get; } = new();
        public ObservableList<Equipment> Equipments { get; } = new();
        public ObservableList<Arsenal> Arsenals { get; } = new();

        public GameStateProxy(GameState gameState)
        {
            GameState = gameState;
            CurrentMapId.Value = gameState.CurrentMapId;

            InitGameplayMaps(gameState);
            InitResources(gameState);
            InitInventories(gameState);
            InitEquipments(gameState);
            InitArsenals(gameState);
            Player = new ReactiveProperty<Player>(new Player(gameState.PlayerData));
            Player.Subscribe(player => gameState.PlayerData = player.Origin);

            GlobalMap = new ReactiveProperty<Maps.GlobalMaps.GlobalMap>(
                new Maps.GlobalMaps.GlobalMap(gameState.GlobalMap));
            GlobalMap.Subscribe(map => gameState.GlobalMap = map.Origin);

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

        private void InitGameplayMaps(GameState gameState)
        {
            gameState.GameplayMaps.ForEach(mapOrigin => Maps.Add(new GameplayMap(mapOrigin)));

            Maps.ObserveAdd().Subscribe(e =>
            {
                var addedMap = e.Value;
                gameState.GameplayMaps.Add(addedMap.Origin);
            });

            Maps.ObserveRemove().Subscribe(e =>
            {
                var removedMap = e.Value;
                var removedMapState =
                    gameState.GameplayMaps.FirstOrDefault(c => c.Id == removedMap.Id);
                gameState.GameplayMaps.Remove(removedMapState);
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
                Debug.Log("Create equip in gameState");
            });

            Equipments.ObserveRemove().Subscribe(e =>
            {
                var removedEquipment = e.Value;
                var removedEquipmentData = gameState.Equipments.FirstOrDefault(c =>
                    c.OwnerId == removedEquipment.OwnerId);
                gameState.Equipments.Remove(removedEquipmentData);
            });
        }

        private void InitArsenals(GameState gameState)
        {
            gameState.Arsenals.ForEach(arsenalData => Arsenals.Add(new Arsenal(arsenalData)));

            Arsenals.ObserveAdd().Subscribe(e =>
            {
                var addedArsenal = e.Value;
                gameState.Arsenals.Add(addedArsenal.Origin);
            });

            Arsenals.ObserveRemove().Subscribe(e =>
            {
                var removedArsenal = e.Value;
                var removedArsenalData = gameState.Arsenals.FirstOrDefault(data =>
                    data.OwnerId == removedArsenal.OwnerId);
                gameState.Arsenals.Remove(removedArsenalData);
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