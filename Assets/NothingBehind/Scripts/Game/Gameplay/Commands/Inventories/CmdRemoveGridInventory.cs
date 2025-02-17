using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Inventories
{
    public class CmdRemoveGridInventory : ICommand
    {
        public readonly int OwnerId;
        public readonly string GridTypeId;
        public CmdRemoveGridInventory(int ownerId, string gridTypeId)
        {
            OwnerId = ownerId;
            GridTypeId = gridTypeId;
        }
    }
}