using System.Linq;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers
{
    public class CmdTriggeredEnemySpawnHandler : ICommandHandler<CmdTriggeredEnemySpawn>
    {
        private readonly GameStateProxy _gameState;

        public CmdTriggeredEnemySpawnHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public bool Handle(CmdTriggeredEnemySpawn command)
        {
            var currentMap = _gameState.Maps.First(currentMap => currentMap.Id == _gameState.CurrentMapId.Value);
            var enemySpawnData = currentMap.EnemySpawns.First(spawnData => spawnData.Id == command.Id);

            enemySpawnData.Triggered.Value = true;

            return true;
        }
    }
}