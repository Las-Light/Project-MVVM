using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.InventoriesCommands
{
    public class CmdRemoveInventory : ICommand
    {
        public readonly int OwnerId;

        public CmdRemoveInventory(int ownerId)
        {
            OwnerId = ownerId;
        }
    }
}