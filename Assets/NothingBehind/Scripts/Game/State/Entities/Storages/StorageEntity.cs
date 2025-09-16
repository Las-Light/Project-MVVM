using NothingBehind.Scripts.Game.State.Inventories;
using R3;

namespace NothingBehind.Scripts.Game.State.Entities.Storages
{
    public class StorageEntity : Entity
    {
        public ReactiveProperty<Inventory> Inventory { get; }

        public StorageEntity(StorageEntityData entityData) : base(entityData)
        {
            Inventory = new ReactiveProperty<Inventory>(new Inventory(entityData.InventoryData));
            Inventory.Skip(1).Subscribe(inventory => entityData.InventoryData = inventory.Origin);
        }
    }
}