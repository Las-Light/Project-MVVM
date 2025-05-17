using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Equipments;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.EquipmentCommands
{
    public class CmdUnequipItem: ICommand
    {
        public readonly int OwnerId;
        public readonly EquipmentSlot Slot;

        public CmdUnequipItem(int ownerId, EquipmentSlot slot)
        {
            OwnerId = ownerId;
            Slot = slot;
        }
    }
}