using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Inventories
{
    public class CmdCreateGridInventory : ICommand
    {
        public readonly string OwnerTypeId;
        public readonly int OwnerId;
        public readonly string GridTypeId;

        public CmdCreateGridInventory(string ownerTypeId, int ownerId, string gridTypeId)
        {
            OwnerTypeId = ownerTypeId;
            OwnerId = ownerId;
            GridTypeId = gridTypeId;
        }
    }
}