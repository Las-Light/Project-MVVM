using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Characters
{
    public class Character
    {
        public int Id { get; }
        public string TypeId { get; }
        public CharacterData Origin { get; }
        
        public ReactiveProperty<Vector3> Position { get; }
        public ReactiveProperty<int> Level { get; }
        public ReactiveProperty<float> Health { get; }

        public Character(CharacterData characterEntity)
        {
            Origin = characterEntity;
            Id = characterEntity.UniqueId;
            TypeId = characterEntity.TypeId;
            Position = new ReactiveProperty<Vector3>(characterEntity.Position);
            Level = new ReactiveProperty<int>(characterEntity.Level);
            Health = new ReactiveProperty<float>(characterEntity.Health);

            Position.Skip(1).Subscribe(value => characterEntity.Position = value);
            Level.Skip(1).Subscribe(value => characterEntity.Level = value);
            Health.Skip(1).Subscribe(value => characterEntity.Health = value);
        }
    }
}