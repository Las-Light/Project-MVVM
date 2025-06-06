using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.EquipmentCommands
{
    public class CmdEquipItem : ICommand
    {
        public readonly int OwnerId;
        public readonly EquipmentSlot Slot;
        public readonly Item Item;
        public CmdEquipItem(int ownerId, EquipmentSlot slot, Item item)
        {
            OwnerId = ownerId;
            Slot = slot;
            Item = item;
        }
    }
}