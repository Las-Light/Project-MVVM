using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands
{
    public class CmdCreateArsenal: ICommand
    {
        public readonly int OwnerId ;

        public CmdCreateArsenal(int ownerId)
        {
            OwnerId = ownerId;
        }
    }
}