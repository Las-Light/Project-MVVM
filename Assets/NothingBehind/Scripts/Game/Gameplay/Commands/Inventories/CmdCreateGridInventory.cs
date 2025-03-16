using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Inventories
{
    public class CmdCreateGridInventory : ICommand
    {
        public readonly EntityType OwnerType;
        public readonly int OwnerId;
        public readonly string GridTypeId;

        public CmdCreateGridInventory(EntityType ownerType, int ownerId, string gridTypeId)
        {
            OwnerType = ownerType;
            OwnerId = ownerId;
            GridTypeId = gridTypeId;
        }
    }
}