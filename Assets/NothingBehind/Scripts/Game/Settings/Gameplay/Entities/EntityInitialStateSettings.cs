using System;
using NothingBehind.Scripts.Game.State.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Entities
{
    [Serializable]
    public class EntityInitialStateSettings
    {
        [field: SerializeField] public EntityType EntityType { get; private set; }
        [field: SerializeField] public string ConfigId { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public Vector3 InitialPosition { get; private set; }

        public EntityInitialStateSettings(EntityType entityType, int level, Vector3 initialPosition, string configId)
        {
            EntityType = entityType;
            Level = level;
            InitialPosition = initialPosition;
            ConfigId = configId;
        }
    }
}