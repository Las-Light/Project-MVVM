using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands
{
    public class CmdRemoveArsenal : ICommand
    {
        public readonly int OwnerId;

        public CmdRemoveArsenal(int ownerId)
        {
            OwnerId = ownerId;
        }
    }
}