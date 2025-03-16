using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.Services
{
    public class InitialGameStateService
    {
        public GameState CreateGameState(GameSettings gameSettings, SceneEnterParams sceneEnterParams)
        {
            var currentMapId = sceneEnterParams.TargetMapId;
            var currentMapSettings = gameSettings.MapsSettings.Maps.First(m => m.MapId == currentMapId);

            var gameState = new GameState();
            gameState.Inventories = new List<InventoryData>();
            gameState.Maps = CreateMaps(gameState, gameSettings);
            gameState.CurrentMapId = currentMapId;
            gameState.PlayerData = CreatePlayer(gameState, gameSettings, currentMapId, currentMapSettings);
            gameState.Resources = new List<ResourceData>()
            {
                new() { Amount = 0, ResourceType = ResourceType.SoftCurrency },
                new() { Amount = 0, ResourceType = ResourceType.HardCurrency }
            };

            return gameState;
        }

        private PlayerData CreatePlayer(GameState gameState, GameSettings gameSettings, MapId currentMapId,
            MapSettings currentMapSettings)
        {
            var hero = new PlayerData()
            {
                UniqueId = gameState.CreateEntityId(),
                EntityType = gameSettings.PlayerSettings.EntityType,
                CurrentMap = currentMapId,
                PositionOnMaps = new List<PositionOnMap>()
                {
                    new()
                    {
                        MapId = currentMapId, 
                        Position = currentMapSettings.InitialStateSettings.PlayerInitialPosition
                    }
                },
                Health = gameSettings.PlayerSettings.Health
            };
            gameState.Inventories.Add(CreateInventories(gameState, gameSettings, hero.EntityType, hero.UniqueId));

            return hero;
        }

        private List<MapData> CreateMaps(GameState gameState, GameSettings gameSettings)
        {
            var maps = new List<MapData>();
            foreach (var map in gameSettings.MapsSettings.Maps)
            {
                maps.Add(CreateMapState(map.MapId, gameState, gameSettings));
            }

            return maps;
        }

        private MapData CreateMapState(MapId mapId, GameState gameState, GameSettings gameSettings)
        {
            var newMapSettings = gameSettings.MapsSettings.Maps.First(m => m.MapId == mapId);
            var newMapInitialStateSettings = newMapSettings.InitialStateSettings;

            var sceneName = newMapSettings.SceneName;
            var newMapState = new MapData
            {
                Id = mapId,
                SceneName = sceneName,
                Characters = InitialCharacters(gameSettings, newMapInitialStateSettings, gameState),
                MapTransfers = InitialMapTransfers(newMapInitialStateSettings),
                EnemySpawns = InitialEnemySpawns(newMapInitialStateSettings)
            };

            return newMapState;
        }

        private List<EnemySpawnData> InitialEnemySpawns(MapInitialStateSettings newMapInitialStateSettings)
        {
            var initialEnemySpawns = new List<EnemySpawnData>();
            foreach (var enemySpawnSettings in newMapInitialStateSettings.EnemySpawns)
            {
                var enemySpawnData = new EnemySpawnData(enemySpawnSettings.Id,
                    enemySpawnSettings.Characters, enemySpawnSettings.Position, enemySpawnSettings.IsTriggered);
                initialEnemySpawns.Add(enemySpawnData);
            }

            return initialEnemySpawns;
        }

        private List<MapTransferData> InitialMapTransfers(MapInitialStateSettings newMapInitialStateSettings)
        {
            var initialMapTransfers = new List<MapTransferData>();
            foreach (var mapTransferSettings in newMapInitialStateSettings.MapTransfers)
            {
                var mapTransferData = new MapTransferData(mapTransferSettings.TargetMapId,
                    mapTransferSettings.Position);
                initialMapTransfers.Add(mapTransferData);
            }

            return initialMapTransfers;
        }

        private List<CharacterData> InitialCharacters(GameSettings gameSettings,
            MapInitialStateSettings newMapInitialStateSettings,
            GameState gameState)
        {
            if (newMapInitialStateSettings.Characters.Count <= 0)
            {
                return new List<CharacterData>();
            }
            var initialCharacters = new List<CharacterData>();
            foreach (var characterSettings in newMapInitialStateSettings.Characters)
            {
                var characterLevelSettings = characterSettings.LevelSettings;
                var initialCharacter = new CharacterData
                {
                    UniqueId = gameState.CreateEntityId(),
                    EntityType = characterSettings.EntityType,
                    Position = characterSettings.Position,
                    Level = characterLevelSettings.Level,
                    Health = characterLevelSettings.Health
                };
                gameState.Inventories.Add(CreateInventories(gameState, gameSettings, initialCharacter.EntityType, initialCharacter.UniqueId));
                initialCharacters.Add(initialCharacter);
            }

            return initialCharacters;
        }

        private InventoryData CreateInventories(GameState gameState, GameSettings gameSettings, EntityType ownerType,
            int ownerId)
        {
            var inventorySettings =
                gameSettings.InventoriesSettings.Inventories.First(settings => settings.OwnerType == ownerType);
            var inventory = new InventoryData()
            {
                OwnerId = ownerId,
                OwnerType = ownerType
            };
            var inventoryGrids = new List<InventoryGridData>();
            foreach (var inventoryGridSettings in inventorySettings.InventoryGrids)
            {
                var inventorySubGrids = CreateSubGrids(gameState, ownerId, inventoryGridSettings.SubGrids);

                var items = CreateItems(gameState, inventoryGridSettings.Items);

                inventoryGrids.Add(new InventoryGridData(ownerId,
                    inventoryGridSettings.GridTypeId,
                    inventoryGridSettings.Width,
                    inventoryGridSettings.Height,
                    inventoryGridSettings.CellSize,
                    inventoryGridSettings.IsSubGrid,
                    inventorySubGrids,
                    items));
            }

            inventory.InventoryGrids = inventoryGrids;

            return inventory;
        }

        private List<InventoryGridData> CreateSubGrids(GameState gameState, int ownerId, List<InventoryGridSettings> subGrids)
        {
            var inventorySubGrids = new List<InventoryGridData>();
            foreach (var subGridSettings in subGrids)
            {
                var subGridData = new InventoryGridData(ownerId,
                    subGridSettings.GridTypeId,
                    subGridSettings.Width,
                    subGridSettings.Height,
                    subGridSettings.CellSize,
                    subGridSettings.IsSubGrid,
                    new List<InventoryGridData>(),
                    CreateItems(gameState, subGridSettings.Items));
                inventorySubGrids.Add(subGridData);
            }

            return inventorySubGrids;
        }

        private List<ItemData> CreateItems(GameState gameState, List<ItemSettings> itemsSettings)
        {
            var items = new List<ItemData>();
            foreach (var itemSettings in itemsSettings)
            {
                var item = new ItemData(gameState.CreateItemId(),
                    itemSettings.ItemType,
                    itemSettings.Width,
                    itemSettings.Height,
                    itemSettings.Weight,
                    itemSettings.CanRotate,
                    itemSettings.IsRotated,
                    itemSettings.IsStackable,
                    itemSettings.MaxStackSize,
                    itemSettings.CurrentStack);
                items.Add(item);
            }

            return items;
        }
    }
}