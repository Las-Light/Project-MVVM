using System;
using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities
{
    public static class EntitiesDataFactory
    {
        public static EntityData CreateEntity(
            EntityInitialStateSettings initialStateSettings,
            EntitiesSettings entitiesSettings)
        {
            switch (initialStateSettings.EntityType)
            {
                case EntityType.Player:
                    return CreateEntity<PlayerEntityData>(initialStateSettings, entitiesSettings);
                case EntityType.Character:
                    return CreateEntity<CharacterEntityData>(initialStateSettings, entitiesSettings);
                case EntityType.Storage:
                    return CreateEntity<StorageEntityData>(initialStateSettings, entitiesSettings);
                default:
                    throw new Exception($"Not implemented entity creation: {initialStateSettings.EntityType}");
            }
        }
        
        private static T CreateEntity<T>(EntityInitialStateSettings initialSettings, EntitiesSettings entitiesSettings) where T : EntityData, new()
        {
            return CreateEntity<T>(
                initialSettings.EntityType,
                initialSettings.ConfigId,
                initialSettings.Level,
                initialSettings.InitialPosition,
                entitiesSettings);
        }

        public static T CreateEntity<T>(
            EntityType type,
            string configId,
            int level,
            Vector3 position,
            EntitiesSettings entitiesSettings)
            where T : EntityData, new()
        {
            var entity = new T
            {
                Type = type,
                ConfigId = configId,
                Level = level,
                Position = position
            };

            switch (entity)
            {
                case PlayerEntityData playerEntityData:
                    UpdatePlayerEntity(playerEntityData, entitiesSettings);
                    break;
                
                case CharacterEntityData characterEntityData:
                    UpdateCharacterEntity(characterEntityData, entitiesSettings);
                    break;
                
                case StorageEntityData storageEntityData:
                    UpdateStorageEntity(storageEntityData, entitiesSettings);
                    break;
                
                default:
                    throw new Exception($"Not implemented entity creation: {type}");
            }

            return entity;
        }

        private static void UpdateCharacterEntity(CharacterEntityData characterEntityData, EntitiesSettings entitiesSettings)
        {
            var characterSettings =
                entitiesSettings.CharactersSettings.Characters.FirstOrDefault(settings => settings.ConfigId == characterEntityData.ConfigId);
            if (characterSettings == null)
            {
                throw new Exception($"Not implemented entity creation - {characterEntityData.Type} with config ID: {characterEntityData.ConfigId}");
            }

            var levelCharacterSettings =
                characterSettings.Levels.FirstOrDefault(levelSettings =>
                    levelSettings.Level == characterEntityData.Level);
            if (levelCharacterSettings == null)
            {
                throw new Exception($"Not implemented entity creation - {characterEntityData.Type} " +
                                    $"with config ID: {characterEntityData.ConfigId} and with level: {characterEntityData.ConfigId}");
            }

            characterEntityData.Health = levelCharacterSettings.Health;
        }

        private static void UpdateStorageEntity(StorageEntityData storageEntityData, EntitiesSettings entitiesSettings)
        {
            var storageSettings =
                entitiesSettings.StoragesSettings.Storages.FirstOrDefault(settings => settings.ConfigId == storageEntityData.ConfigId);
            if (storageSettings == null)
            {
                throw new Exception($"Not implemented entity creation - {storageEntityData.Type} with config ID: {storageEntityData.ConfigId}");
            }

            var levelStorageSettings =
                storageSettings.Levels.FirstOrDefault(levelSettings =>
                    levelSettings.Level == storageEntityData.Level);
            if (levelStorageSettings == null)
            {
                throw new Exception($"Not implemented entity creation - {storageEntityData.Type} " +
                                    $"with config ID: {storageEntityData.ConfigId} and with level: {storageEntityData.ConfigId}");
            }
        }

        private static void UpdatePlayerEntity(PlayerEntityData playerEntityData, EntitiesSettings entitiesSettings)
        {
            var playerSettings = entitiesSettings.PlayerSettings;
            var levelSettings =
                playerSettings.Levels.FirstOrDefault(levelSettings =>
                    levelSettings.Level == playerEntityData.Level);
            if (levelSettings == null)
            {
                throw new Exception($"Not implemented entity creation - {playerEntityData.Type} " +
                                    $"with config ID: {playerEntityData.ConfigId} and with level: {playerEntityData.ConfigId}");
            }

            playerEntityData.PositionOnMaps = new List<PositionOnMapData>();
            playerEntityData.Health = levelSettings.Health;
        }
    }
}