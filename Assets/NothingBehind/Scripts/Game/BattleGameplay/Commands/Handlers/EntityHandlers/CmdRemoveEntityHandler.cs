using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.EntityCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.EntityHandlers
{
    public class CmdRemoveEntityHandler : ICommandHandler<CmdRemoveEntity>
    {
        private readonly GameStateProxy _gameState;
        
        public CmdRemoveEntityHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        
        public CommandResult Handle(CmdRemoveEntity command)
        {
            var currentMapId = _gameState.CurrentMapId.CurrentValue;
            var currentMap = _gameState.Maps.FirstOrDefault(m => m.Id == currentMapId);
            if (currentMap == null)
            {
                Debug.Log($"Couldn't find Mapstate for ID: {currentMapId}");
                return new CommandResult(false);
            }

            var removedEntity = currentMap.Entities.FirstOrDefault(entity => entity.UniqueId == command.UniqueId);
            if (removedEntity == null)
            {
                Debug.Log($"Couldn't find Storage for ID: {command.UniqueId}");
                return new CommandResult(command.UniqueId, false);
            }

            currentMap.Entities.Remove(removedEntity);
            return new CommandResult(command.UniqueId, true);
        }
    }
}