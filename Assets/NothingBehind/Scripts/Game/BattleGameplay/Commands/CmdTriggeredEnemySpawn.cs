using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands
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