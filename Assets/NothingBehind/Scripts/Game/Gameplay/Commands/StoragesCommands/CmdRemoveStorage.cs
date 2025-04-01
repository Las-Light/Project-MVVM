using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.StoragesCommands
{
    public class CmdRemoveStorage : ICommand
    {
        public readonly int Id;
        public readonly InventoryService InventoryService;

        public CmdRemoveStorage(int id, InventoryService inventoryService)
        {
            Id = id;
            InventoryService = inventoryService;
        }
    }
}