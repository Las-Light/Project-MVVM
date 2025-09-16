using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.EntityCommands
{
    public class CmdRemoveEntity : ICommand
    {
        public readonly int UniqueId;

        public CmdRemoveEntity(int uniqueId)
        {
            UniqueId = uniqueId;
        }
    }
}