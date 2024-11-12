using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Characters
{
    public class CharacterEntityProxy
    {
        public int Id { get; }
        public string TypeId { get; }
        public int Level { get; }
        public CharacterEntity Origin { get; }
        
        public ReactiveProperty<Vector3Int> Position { get; }
        public ReactiveProperty<float> Health { get; }

        public CharacterEntityProxy(CharacterEntity characterEntity)
        {
            Origin = characterEntity;
            Id = characterEntity.Id;
            TypeId = characterEntity.TypeID;
            Level = characterEntity.Level;
            Position = new ReactiveProperty<Vector3Int>(characterEntity.Position);
            Health = new ReactiveProperty<float>(characterEntity.Health);

            Position.Skip(1).Subscribe(value => characterEntity.Position = value);
            Health.Skip(1).Subscribe(value => characterEntity.Health = value);
        }
    }
}