using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.InventoriesCommands
{
    public class CmdCreateInventory : ICommand
    {
        public readonly EntityType OwnerType;
        public readonly int OwnerId;

        public CmdCreateInventory(EntityType ownerType, int ownerId)
        {
            OwnerType = ownerType;
            OwnerId = ownerId;
        }
    }
}