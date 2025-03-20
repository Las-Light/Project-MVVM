using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.PlayerHandlers
{
    public class CmdUpdatePlayerPosOnMapHandler : ICommandHandler<CmdUpdatePlayerPosOnMap>
    {
        private readonly GameStateProxy _gameState;

        public CmdUpdatePlayerPosOnMapHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public bool Handle(CmdUpdatePlayerPosOnMap command)
        {
            var currentPosOnMap =
                _gameState.Player.Value.PositionOnMaps.First(posOnMap => posOnMap.MapId == command.CurrentMap);
            currentPosOnMap.Position.Value = command.Position;

            return true;
        }
    }
}