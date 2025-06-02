using System.Linq;
using NothingBehind.Scripts.Game.GlobalMap.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.GlobalMap.Commands.Handlers.PlayerHandlers
{
    public class CmdUpdatePlayerPosOnGlobalMapHandler: ICommandHandler<CmdUpdatePlayerPosOnGlobalMap>
    {
        private readonly GameStateProxy _gameState;

        public CmdUpdatePlayerPosOnGlobalMapHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public CommandResult Handle(CmdUpdatePlayerPosOnGlobalMap command)
        {
            var currentPosOnMap =
                _gameState.Player.Value.PositionOnMaps.First(posOnMap => posOnMap.MapId == command.CurrentMap);
            currentPosOnMap.Position.Value = command.Position;

            return new CommandResult(true);
        }
    }
}