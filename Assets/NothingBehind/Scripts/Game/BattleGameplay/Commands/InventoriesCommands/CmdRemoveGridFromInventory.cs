using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Inventories.Grids;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.InventoriesCommands
{
    public class CmdRemoveGridFromInventory : ICommand
    {
        public readonly EntityType EntityType;
        public readonly int OwnerId;
        public readonly InventoryGrid Grid;
        public CmdRemoveGridFromInventory(EntityType entityType, int ownerId, InventoryGrid grid)
        {
            EntityType = entityType;
            OwnerId = ownerId;
            Grid = grid;
        }
    }
}