using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Characters
{
    public class CharacterEntityProxy
    {
        public int Id { get; }
        public string TypeId { get; }
        public CharacterEntity Origin { get; }
        
        public ReactiveProperty<Vector3> Position { get; }
        public ReactiveProperty<int> Level { get; }
        public ReactiveProperty<float> Health { get; }

        public CharacterEntityProxy(CharacterEntity characterEntity)
        {
            Origin = characterEntity;
            Id = characterEntity.Id;
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