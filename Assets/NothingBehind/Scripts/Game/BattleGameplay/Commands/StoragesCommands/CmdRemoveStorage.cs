using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.StoragesCommands
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