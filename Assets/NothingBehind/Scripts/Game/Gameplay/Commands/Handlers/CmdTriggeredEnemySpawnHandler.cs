using System.Linq;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers
{
    public class CmdTriggeredEnemySpawnHandler : ICommandHandler<CmdTriggeredEnemySpawn>
    {
        private readonly GameStateProxy _gameState;

        public CmdTriggeredEnemySpawnHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public CommandResult Handle(CmdTriggeredEnemySpawn command)
        {
            var currentMap = _gameState.Maps.First(currentMap => currentMap.Id == _gameState.CurrentMapId.Value);
            var enemySpawnData = currentMap.EnemySpawns.First(spawnData => spawnData.Id == command.Id);

            enemySpawnData.Triggered.Value = true;

            return new CommandResult(true);
        }
    }
}