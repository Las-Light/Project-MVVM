using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventories.Grids;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.InventoriesCommands
{
    public class CmdAddGridToInventory : ICommand
    {
        public readonly int OwnerId;
        public readonly InventoryGrid Grid;

        public CmdAddGridToInventory(int ownerId, InventoryGrid grid)
        {
            OwnerId = ownerId;
            Grid = grid;
        }
    }
}