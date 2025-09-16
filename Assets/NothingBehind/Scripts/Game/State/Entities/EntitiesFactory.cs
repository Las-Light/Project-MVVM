using System;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities
{
    public static class EntitiesFactory
    {
        public static Entity CreateEntity(EntityData entityData)
        {
            switch (entityData.Type)
            {
                case EntityType.Player:
                    return new PlayerEntity(entityData as PlayerEntityData);
                case EntityType.Character:
                    return new CharacterEntity(entityData as CharacterEntityData);
                case EntityType.Storage:
                    return new StorageEntity(entityData as StorageEntityData);
                default:
                    throw new Exception("Unsupported entity type" + entityData.Type);
            }
        }
    }
}