using NothingBehind.Scripts.Game.BattleGameplay.Commands.EquipmentCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.EquipmentHandlers
{
    public class CmdEquipItemHandler: ICommandHandler<CmdEquipItem>
    {
        public CommandResult Handle(CmdEquipItem command)
        {
            command.Slot.Equip(command.Item);
            return new CommandResult(command.OwnerId, true);
        }
    }
}