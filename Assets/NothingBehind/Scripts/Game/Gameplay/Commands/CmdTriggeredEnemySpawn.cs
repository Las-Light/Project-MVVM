using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdTriggeredEnemySpawn : ICommand
    {
        public readonly string Id;

        public CmdTriggeredEnemySpawn(string id)
        {
            Id = id;
        }
    }
}