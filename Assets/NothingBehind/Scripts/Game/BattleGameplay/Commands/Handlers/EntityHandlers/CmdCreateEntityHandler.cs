using System;
using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.EntityCommands;
using NothingBehind.Scripts.Game.GameRoot.Commands.Handlers;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.EntityHandlers
{
    public class CmdCreateEntityHandler : ICommandHandler<CmdCreateEntity>
    {
        private readonly GameStateProxy _gameState;
        private readonly GameSettings _gameSettings;

        public CmdCreateEntityHandler(GameStateProxy gameState, GameSettings gameSettings)
        {
            _gameState = gameState;
            _gameSettings = gameSettings;
        }

        public CommandResult Handle(CmdCreateEntity command)
        {
            int createdEntityId;
            var entitySettings = _gameSettings.EntitiesSettings;
            var uniqueId = _gameState.CreateEntityId();
            var currentMapId = _gameState.CurrentMapId.CurrentValue;
            var currentMap = _gameState.Maps.FirstOrDefault(map => map.Id == currentMapId);
            if (currentMap == null)
            {
                Debug.Log($"Couldn't find map with MapId - {currentMapId}");
                return new CommandResult(false);
            }
            switch (command.EntityType)
            {
                case EntityType.Player:
                    var playerEntityData = EntitiesDataFactory.CreateEntity<PlayerEntityData>(
                        command.EntityType,
                        command.ConfigId,
                        command.Level,
                        command.Position,
                        entitySettings);
                    playerEntityData.UniqueId = createdEntityId = uniqueId;
                    InitialEntitySystem(playerEntityData, _gameSettings);
                    
                    var playerEntity = EntitiesFactory.CreateEntity(playerEntityData);
                    _gameState.Player.OnNext(playerEntity as PlayerEntity);
                    break;
                case EntityType.Character:
                    var characterEntityData = EntitiesDataFactory.CreateEntity<CharacterEntityData>(
                        command.EntityType,
                        command.ConfigId,
                        command.Level,
                        command.Position,
                        entitySettings);
                    characterEntityData.UniqueId = createdEntityId = uniqueId;
                    InitialEntitySystem(characterEntityData, _gameSettings);

                    var characterEntity = EntitiesFactory.CreateEntity(
                        characterEntityData);
                    currentMap.Entities.Add(characterEntity);
                    break;
                case EntityType.Storage:
                    var storageEntityData = EntitiesDataFactory.CreateEntity<StorageEntityData>(
                        command.EntityType,
                        command.ConfigId,
                        command.Level,
                        command.Position,
                        entitySettings);
                    storageEntityData.UniqueId = createdEntityId = uniqueId;
                    InitialEntitySystem(storageEntityData, _gameSettings);
                    
                    var storageEntity = EntitiesFactory.CreateEntity(storageEntityData);
                    currentMap.Entities.Add(storageEntity);
                    break;
                default:
                    throw new Exception($"Couldn't create Entity with EntityType - {command.EntityType}");
            }

            return new CommandResult(createdEntityId, true);
        }
        
        private void InitialEntitySystem(EntityData entityData, GameSettings gameSettings)
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
                    FillEquipmentSlots(characterEntityData.EquipmentData, characterEntityData.Type, gameSettings);
                    FillArsenalUnarmedData(characterEntityData.ArsenalData, gameSettings);
                    break;
                case PlayerEntityData playerEntityData:
                    playerEntityData.InventoryData = InventoryDataFactory.CreateInventoryData(playerEntityData.Type, playerEntityData.UniqueId);
                    playerEntityData.EquipmentData = EquipmentDataFactory.CreateEquipmentData(gameSettings,
                        playerEntityData.Type, playerEntityData.UniqueId);
                    playerEntityData.ArsenalData =
                        ArsenalDataFactory.CreateArsenalData(playerEntityData.Type, playerEntityData.UniqueId);
                    FillEquipmentSlots(playerEntityData.EquipmentData, playerEntityData.Type, gameSettings);
                    FillArsenalUnarmedData(playerEntityData.ArsenalData, gameSettings);
                    break;
                case StorageEntityData storageEntityData:
                    storageEntityData.InventoryData =
                        InventoryDataFactory.CreateInventoryData(storageEntityData.Type, storageEntityData.UniqueId);
                    InitialInventoryGrids(storageEntityData.Type, gameSettings, storageEntityData.InventoryData);
                    break;
            }
        }
        private void FillEquipmentSlots( 
            EquipmentData equipmentData,
            EntityType ownerType,
            GameSettings gameSettings)
        {
            var equipmentSettings =
                gameSettings.EquipmentsSettings.AllEquipments.First(settings =>
                    settings.EntityType == ownerType);
            foreach (var settingsSlot in equipmentSettings.Slots)
            {
                var slot = new EquipmentSlotData();
                slot.SlotType = settingsSlot.SlotType;
                slot.ItemType = settingsSlot.ItemType;
                slot.Width = settingsSlot.Width;
                slot.Height = settingsSlot.Height;
                if (settingsSlot.EquippedItemSettings != null)
                {
                    slot.EquippedItem =
                        ItemsDataFactory.CreateItemData(_gameState.GameState, gameSettings, settingsSlot.EquippedItemSettings);
                }
                equipmentData.Slots.Add(slot);
            }
        }
        
        private void InitialInventoryGrids(
            EntityType ownerType,
            GameSettings gameSettings,
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
                    var grid = InventoryGridsDataFactory.CreateInventorGridData(_gameState.GameState, gridSettings);
                    inventoryData.InventoryGrids.Add(grid);
                }
            }
        }

        private void FillArsenalUnarmedData(
            ArsenalData arsenalData,
            GameSettings gameSettings)
        {
            var weaponSettings =
                gameSettings.WeaponsSettings.WeaponConfigs.First(settings => settings.WeaponType == WeaponType.Unarmed);
            var unarmedData = WeaponDataFactory.CreateWeaponData(_gameState.GameState, gameSettings, _gameState.CreateItemId(), weaponSettings.WeaponName);
            arsenalData.Weapons.Add(unarmedData);
        }
    }
}