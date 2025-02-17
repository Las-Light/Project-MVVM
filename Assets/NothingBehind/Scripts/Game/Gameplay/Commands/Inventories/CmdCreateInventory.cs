using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Inventories
{
    public class CmdCreateInventory : ICommand
    {
        public readonly string OwnerTypeId;
        public readonly int OwnerId;

        public CmdCreateInventory(string ownerTypeId, int ownerId)
        {
            OwnerTypeId = ownerTypeId;
            OwnerId = ownerId;
        }
    }
}