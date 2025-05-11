using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;

namespace NothingBehind.Scripts.Game.GameRoot.Services
{
    public class InitialGameStateService
    {
        public GameState CreateGameState(GameSettings gameSettings, 
            SceneEnterParams sceneEnterParams)
        {
            var currentMapId = sceneEnterParams.TargetMapId;
            var currentMapSettings = gameSettings.MapsSettings.Maps.First(m => m.MapId == currentMapId);

            var gameState = new GameState();
            gameState.Equipments = new List<EquipmentData>();
            gameState.Inventories = new List<InventoryData>();
            gameState.Arsenals = new List<ArsenalData>();
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
            var player = new PlayerData()
            {
                UniqueId = gameState.CreateEntityId(),
                EntityType = gameSettings.PlayerSettings.EntityType,
                CurrentMapId = currentMapId,
                PositionOnMaps = new List<PositionOnMapData>()
                {
                    new()
                    {
                        MapId = currentMapId, 
                        Position = currentMapSettings.InitialStateSettings.PlayerInitialPosition
                    }
                },
                Health = gameSettings.PlayerSettings.Health
            };
            gameState.Equipments.Add(CreateEquipmentData(gameState, gameSettings, player.EntityType, player.UniqueId));
            gameState.Inventories.Add(CreateInventoryData(player.EntityType, player.UniqueId));
            gameState.Arsenals.Add(CreateArsenalData(gameState, gameSettings, player.UniqueId));

            return player;
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
                EnemySpawns = InitialEnemySpawns(newMapInitialStateSettings),
                Storages = new List<StorageData>()
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
                gameState.Equipments.Add(CreateEquipmentData(gameState, gameSettings, initialCharacter.EntityType, initialCharacter.UniqueId));
                gameState.Inventories.Add(CreateInventoryData(initialCharacter.EntityType, initialCharacter.UniqueId));
                gameState.Arsenals.Add(CreateArsenalData(gameState, gameSettings, initialCharacter.UniqueId));
                initialCharacters.Add(initialCharacter);
            }

            return initialCharacters;
        }

        private InventoryData CreateInventoryData(EntityType ownerType,
            int ownerId)
        {
            var inventory = new InventoryData()
            {
                OwnerId = ownerId,
                OwnerType = ownerType
            };
            var inventoryGrids = new List<InventoryGridData>();

            inventory.InventoryGrids = inventoryGrids;

            return inventory;
        }

        private EquipmentData CreateEquipmentData(GameState gameState,
            GameSettings gameSettings,
            EntityType ownerType,
            int ownerId)
        {
            var equipmentSettings =
                gameSettings.EquipmentsSettings.AllEquipments.First(settings => settings.EntityType == ownerType);
            var equipmentSlots = new List<EquipmentSlotData>();
            foreach (var settingsSlot in equipmentSettings.Slots)
            {
                var slot = new EquipmentSlotData();
                slot.SlotType = settingsSlot.SlotType;
                slot.ItemType = settingsSlot.ItemType;
                slot.Width = settingsSlot.Width;
                slot.Height = settingsSlot.Height;
                if (settingsSlot.EquippedItemSettings != null)
                {
                    slot.EquippedItem = ItemsDataFactory.CreateItemData(gameState, gameSettings, settingsSlot.EquippedItemSettings);
                }
                equipmentSlots.Add(slot);
            }

            var equipment = new EquipmentData
            {
                OwnerId = ownerId,
                Slots = equipmentSlots,
                Width = equipmentSettings.Width,
                Height = equipmentSettings.Height
            };
            return equipment;
        }

        private ArsenalData CreateArsenalData(GameState gameState, GameSettings gameSettings, int ownerId)
        {
            var arsenalData = new ArsenalData
            {
                OwnerId = ownerId,
                Weapons = new List<WeaponData>()
            };
            var weaponSettings =
                gameSettings.WeaponsSettings.WeaponConfigs.First(settings => settings.WeaponType == WeaponType.Unarmed);
            var unarmedData = WeaponDataFactory.CreateWeaponData(gameState, gameSettings, gameState.GlobalItemId, weaponSettings.WeaponName);
            arsenalData.Weapons.Add(unarmedData);
            return arsenalData;
        }
    }
}