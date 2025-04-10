using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.CharactersCommands
{
    public class CmdRemoveCharacter : ICommand
    {
        public readonly int Id;
        public readonly InventoryService InventoryService;
        public readonly EquipmentService EquipmentService;
        public readonly ArsenalService ArsenalService;

        public CmdRemoveCharacter(int id, 
            InventoryService inventoryService, 
            EquipmentService equipmentService,
            ArsenalService arsenalService)
        {
            Id = id;
            InventoryService = inventoryService;
            EquipmentService = equipmentService;
            ArsenalService = arsenalService;
        }
    }
}