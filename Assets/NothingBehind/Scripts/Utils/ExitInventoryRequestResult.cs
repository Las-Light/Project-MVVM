using NothingBehind.Scripts.Game.State.Entities;

namespace NothingBehind.Scripts.Utils
{
    public readonly struct ExitInventoryRequestResult
    {
        public readonly bool IsEmptyInventory;
        public readonly int OwnerId;
        public readonly EntityType EntityType; 


        public ExitInventoryRequestResult(bool isEmptyInventory, int ownerId, EntityType entityType)
        {
            IsEmptyInventory = isEmptyInventory;
            OwnerId = ownerId;
            EntityType = entityType;
        }
    }
}