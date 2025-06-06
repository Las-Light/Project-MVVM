using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.ArsenalHandlers
{
    public class CmdRemoveArsenalHandler : ICommandHandler<CmdRemoveArsenal>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveArsenalHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public CommandResult Handle(CmdRemoveArsenal command)
        {
            var removedArsenal = _gameState.Arsenals.FirstOrDefault(arsenal => arsenal.OwnerId == command.OwnerId);
            if (removedArsenal != null)
            {
                _gameState.Arsenals.Remove(removedArsenal);
                return new CommandResult(command.OwnerId, true);
            }
            Debug.LogError($"Arsenal with ownerId {command.OwnerId} not found");
            return new CommandResult(command.OwnerId, false);
        }
    }
}