using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventories.Grids;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.InventoriesCommands
{
    public class CmdRemoveGridFromInventory : ICommand
    {
        public readonly int OwnerId;
        public readonly InventoryGrid Grid;
        public CmdRemoveGridFromInventory(int ownerId, InventoryGrid grid)
        {
            OwnerId = ownerId;
            Grid = grid;
        }
    }
}