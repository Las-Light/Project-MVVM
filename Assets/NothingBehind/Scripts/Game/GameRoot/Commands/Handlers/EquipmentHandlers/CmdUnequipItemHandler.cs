using NothingBehind.Scripts.Game.GameRoot.Commands.EquipmentCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.EquipmentHandlers
{
    public class CmdUnequipItemHandler: ICommandHandler<CmdUnequipItem>
    {
        public CommandResult Handle(CmdUnequipItem command)
        {
            if (command.Slot.Unequip())
            {
                return new CommandResult(command.OwnerId, true);
            }

            return new CommandResult(command.OwnerId, false);
        }
    }
}