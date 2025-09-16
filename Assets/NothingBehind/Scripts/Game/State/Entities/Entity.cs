using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities
{
    public abstract class Entity
    {
        public EntityData Origin { get; }
        public EntityType EntityType => Origin.Type;
        public int UniqueId => Origin.UniqueId;
        public string ConfigId => Origin.ConfigId;
        public readonly ReactiveProperty<int> Level;
        public readonly ReactiveProperty<Vector3> Position;

        public Entity(EntityData entityData)
        {
            Origin = entityData;

            Level = new ReactiveProperty<int>(entityData.Level);
            Position = new ReactiveProperty<Vector3>(entityData.Position);
            Level.Subscribe(newLevel => { entityData.Level = newLevel; });
            Position.Subscribe(newPosition => { entityData.Position = newPosition; });
        }
    }
}