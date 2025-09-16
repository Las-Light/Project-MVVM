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
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

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
            gameState.Maps = CreateMaps(gameState, gameSettings);
            gameState.CurrentMapId = currentMapId;
            gameState.PlayerEntityData = CreatePlayer(gameState, gameSettings, currentMapSettings);
            gameState.Resources = new List<ResourceData>()
            {
                new() { Amount = 0, ResourceType = ResourceType.SoftCurrency },
                new() { Amount = 0, ResourceType = ResourceType.HardCurrency }
            };

            return gameState;
        }

        private PlayerEntityData CreatePlayer(GameState gameState, GameSettings gameSettings,
            MapSettings currentMapSettings)
        {
            var uniqueId = gameState.CreateEntityId();
            var playerSettings = gameSettings.EntitiesSettings.PlayerSettings;
            var playerEntityData = EntitiesDataFactory.CreateEntity<PlayerEntityData>(
                playerSettings.EntityType,
                playerSettings.ConfigId,
                0,
                currentMapSettings.InitialMapSettings.PlayerInitialPosition,
                gameSettings.EntitiesSettings);
            playerEntityData.UniqueId = uniqueId;
            InitialPlayerPositionOnMap(playerEntityData, currentMapSettings.MapId, playerEntityData.Position);
            InitialEntitySystem(gameState, playerEntityData, gameSettings);

            return playerEntityData;
        }

        private List<MapData> CreateMaps(GameState gameState, GameSettings gameSettings)
        {
            var maps = new List<MapData>();
            foreach (var map in gameSettings.MapsSettings.Maps)
            {
                var mapInitialSettings = map.InitialMapSettings;
                var initialMap = MapDataFactory.CreateMapData(map.InitialMapSettings, gameSettings.MapsSettings);
                initialMap.Id = map.MapId;
                initialMap.SceneName = map.SceneName;
                initialMap.Entities = InitialEntity(gameSettings, mapInitialSettings, gameState);
                initialMap.EnemySpawns = InitialEnemySpawns(mapInitialSettings);
                initialMap.MapTransfers = InitialMapTransfers(mapInitialSettings);
                
                maps.Add(initialMap);
            }

            return maps;
        }

        private List<EnemySpawnData> InitialEnemySpawns(MapInitialStateSettings newMapInitialStateSettings)
        {
            if (newMapInitialStateSettings.EnemySpawns.Count <= 0)
            {
                return new List<EnemySpawnData>();
            }
            
            var initialEnemySpawns = new List<EnemySpawnData>();
            foreach (var enemySpawnSettings in newMapInitialStateSettings.EnemySpawns)
            {
                var enemySpawnData = new EnemySpawnData(enemySpawnSettings.Id,
                    enemySpawnSettings.Characters,
                    enemySpawnSettings.Position,
                    enemySpawnSettings.IsTriggered);
                initialEnemySpawns.Add(enemySpawnData);
            }

            return initialEnemySpawns;
        }

        private List<MapTransferData> InitialMapTransfers(MapInitialStateSettings newMapInitialStateSettings)
        {
            if (newMapInitialStateSettings.MapTransfers.Count <= 0)
            {
                return new List<MapTransferData>();
            }
            
            var initialMapTransfers = new List<MapTransferData>();
            foreach (var mapTransferData in newMapInitialStateSettings.MapTransfers)
            {
                initialMapTransfers.Add(mapTransferData);
            }

            return initialMapTransfers;
        }

        private List<EntityData> InitialEntity(GameSettings gameSettings,
            MapInitialStateSettings newMapInitialStateSettings,
            GameState gameState)
        {
            if (newMapInitialStateSettings.Entities.Count <= 0)
            {
                return new List<EntityData>();
            }

            var initialEntities = new List<EntityData>();
            foreach (var initialStateSettings in newMapInitialStateSettings.Entities)
            {
                var uniqueId = gameState.CreateEntityId();
                var initialEntity =
                    EntitiesDataFactory.CreateEntity(initialStateSettings, gameSettings.EntitiesSettings);
                initialEntity.UniqueId = uniqueId;
                InitialEntitySystem(gameState, initialEntity, gameSettings);

                initialEntities.Add(initialEntity);
            }

            return initialEntities;
        }

        private void InitialPlayerPositionOnMap(PlayerEntityData playerEntityData,
            MapId requiredMap, Vector3 initialPosition)
        {
            var newPosOnMap = new PositionOnMapData()
            {
                MapId = requiredMap,
                Position = initialPosition
            };

            playerEntityData.PositionOnMaps.Add(newPosOnMap);
        }

        private void InitialEntitySystem(GameState gameState, EntityData entityData, GameSettings gameSettings)
        {
            switch (entityData)
            {
                case CharacterEntityData characterEntityData:
                    characterEntityData.InventoryData =
                        InventoryDataFactory.CreateInventoryData(characterEntityData.Type,
                            characterEntityData.UniqueId);
                    characterEntityData.EquipmentData = EquipmentDataFactory.CreateEquipmentData(gameSettings,
                        characterEntityData.Type, characterEntityData.UniqueId);
                    characterEntityData.ArsenalData =
                        ArsenalDataFactory.CreateArsenalData(characterEntityData.Type, characterEntityData.UniqueId);
                    FillEquipmentSlots(gameState, characterEntityData.EquipmentData, characterEntityData.Type,
                        gameSettings);
                    FillArsenalUnarmedData(gameState, characterEntityData.ArsenalData, gameSettings);
                    break;
                case PlayerEntityData playerEntityData:
                    playerEntityData.InventoryData =
                        InventoryDataFactory.CreateInventoryData(playerEntityData.Type, playerEntityData.UniqueId);
                    playerEntityData.EquipmentData = EquipmentDataFactory.CreateEquipmentData(gameSettings,
                        playerEntityData.Type, playerEntityData.UniqueId);
                    playerEntityData.ArsenalData =
                        ArsenalDataFactory.CreateArsenalData(playerEntityData.Type, playerEntityData.UniqueId);
                    FillEquipmentSlots(gameState, playerEntityData.EquipmentData, playerEntityData.Type, gameSettings);
                    FillArsenalUnarmedData(gameState, playerEntityData.ArsenalData, gameSettings);
                    break;
                case StorageEntityData storageEntityData:
                    storageEntityData.InventoryData =
                        InventoryDataFactory.CreateInventoryData(storageEntityData.Type, storageEntityData.UniqueId);
                    InitialInventoryGrids(storageEntityData.Type, gameSettings, gameState,
                        storageEntityData.InventoryData);
                    break;
            }
        }

        private void FillEquipmentSlots(GameState gameState,
            EquipmentData equipmentData,
            EntityType ownerType,
            GameSettings gameSettings)
        {
            var equipmentSettings =
                gameSettings.EquipmentsSettings.AllEquipments.First(settings =>
                    settings.EntityType == ownerType);
            Debug.Log("Slot count " + equipmentData.Slots.Count);
            int n = 1;
            foreach (var settingsSlot in equipmentSettings.Slots)
            {
                Debug.Log(n++);
                var slot = new EquipmentSlotData();
                slot.SlotType = settingsSlot.SlotType;
                slot.ItemType = settingsSlot.ItemType;
                slot.Width = settingsSlot.Width;
                slot.Height = settingsSlot.Height;
                if (settingsSlot.EquippedItemSettings != null)
                {
                    slot.EquippedItem =
                        ItemsDataFactory.CreateItemData(gameState, gameSettings, settingsSlot.EquippedItemSettings);
                    Debug.Log(slot.EquippedItem.ItemType);
                }

                equipmentData.Slots.Add(slot);
            }
        }

        private void InitialInventoryGrids(
            EntityType ownerType,
            GameSettings gameSettings,
            GameState gameState,
            InventoryData inventoryData)
        {
            // Для сущностей у которых есть инвентарь, но нет EquipmentSystem создаем сетки
            var inventorySettings =
                gameSettings.InventoriesSettings.Inventories.FirstOrDefault(settings =>
                    settings.OwnerType == ownerType);

            if (inventorySettings != null)
            {
                var gridsSettings = inventorySettings.GridsSettings;
                foreach (var gridSettings in gridsSettings)
                {
                    var grid = InventoryGridsDataFactory.CreateInventorGridData(gameState, gridSettings);
                    inventoryData.InventoryGrids.Add(grid);
                }
            }
        }

        private void FillArsenalUnarmedData(GameState gameState,
            ArsenalData arsenalData,
            GameSettings gameSettings)
        {
            var weaponSettings =
                gameSettings.WeaponsSettings.WeaponConfigs.First(settings => settings.WeaponType == WeaponType.Unarmed);
            var unarmedData = WeaponDataFactory.CreateWeaponData(gameState, gameSettings, gameState.CreateItemId(),
                weaponSettings.WeaponName);
            arsenalData.Weapons.Add(unarmedData);
        }

        // private InventoryData CreateInventoryData(EntityType ownerType,
        //     int ownerId)
        // {
        //     var inventory = new InventoryData()
        //     {
        //         OwnerId = ownerId,
        //         OwnerType = ownerType
        //     };
        //     var inventoryGrids = new List<InventoryGridData>();
        //
        //     inventory.InventoryGrids = inventoryGrids;
        //
        //     return inventory;
        // }

        // private EquipmentData CreateEquipmentData(GameState gameState,
        //     GameSettings gameSettings,
        //     EntityType ownerType,
        //     int ownerId)
        // {
        //     var equipmentSettings =
        //         gameSettings.EquipmentsSettings.AllEquipments.First(settings => settings.EntityType == ownerType);
        //     var equipmentSlots = new List<EquipmentSlotData>();
        //     foreach (var settingsSlot in equipmentSettings.Slots)
        //     {
        //         var slot = new EquipmentSlotData();
        //         slot.SlotType = settingsSlot.SlotType;
        //         slot.ItemType = settingsSlot.ItemType;
        //         slot.Width = settingsSlot.Width;
        //         slot.Height = settingsSlot.Height;
        //         if (settingsSlot.EquippedItemSettings != null)
        //         {
        //             slot.EquippedItem = ItemsDataFactory.CreateItemData(gameState, gameSettings, settingsSlot.EquippedItemSettings);
        //         }
        //         equipmentSlots.Add(slot);
        //     }
        //
        //     var equipment = new EquipmentData
        //     {
        //         OwnerId = ownerId,
        //         Slots = equipmentSlots,
        //         Width = equipmentSettings.Width,
        //         Height = equipmentSettings.Height
        //     };
        //     return equipment;
        // }

        // private ArsenalData CreateArsenalData(GameState gameState, 
        //     GameSettings gameSettings, 
        //     EntityType ownerType,
        //     int ownerId)
        // {
        //     var arsenalData = new ArsenalData
        //     {
        //         OwnerId = ownerId,
        //         OwnerType = ownerType,
        //         CurrentWeaponSlot = SlotType.Weapon1,
        //         Weapons = new List<WeaponData>()
        //     };
        //     var weaponSettings =
        //         gameSettings.WeaponsSettings.WeaponConfigs.First(settings => settings.WeaponType == WeaponType.Unarmed);
        //     var unarmedData = WeaponDataFactory.CreateWeaponData(gameState, gameSettings, gameState.GlobalItemId, weaponSettings.WeaponName);
        //     arsenalData.Weapons.Add(unarmedData);
        //     return arsenalData;
        // }
    }
}