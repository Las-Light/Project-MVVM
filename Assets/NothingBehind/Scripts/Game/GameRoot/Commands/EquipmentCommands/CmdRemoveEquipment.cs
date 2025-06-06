using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.EquipmentCommands
{
    public class CmdRemoveEquipment : ICommand
    {
        public readonly int OwnerId;

        public CmdRemoveEquipment(int ownerId)
        {
            OwnerId = ownerId;
        }
    }
}