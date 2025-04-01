using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Storages
{
    public class Storage : Entity
    {
        public int Id { get; set; }
        public EntityType EntityType { get; set; }
        public ReactiveProperty<Vector3> Position { get; set; }
        public StorageData Origin  { get; set; }

        public Storage(StorageData data)
        {
            Id = data.UniqueId;
            EntityType = data.EntityType;
            Position = new ReactiveProperty<Vector3>(data.Position);
            Origin = data;
        }
    }
}