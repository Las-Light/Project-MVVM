using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Weapons
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