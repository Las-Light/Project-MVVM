using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Weapons
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