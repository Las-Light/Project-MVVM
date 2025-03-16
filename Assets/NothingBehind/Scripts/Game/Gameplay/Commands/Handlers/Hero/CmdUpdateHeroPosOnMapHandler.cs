using NothingBehind.Scripts.Game.Gameplay.Commands.Hero;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Hero
{
    public class CmdUpdateHeroPosOnMapHandler : ICommandHandler<CmdUpdatePlayerPosOnMap>
    {
        private readonly GameStateProxy _gameState;

        public CmdUpdateHeroPosOnMapHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public bool Handle(CmdUpdatePlayerPosOnMap command)
        {
            _gameState.Hero.Value.CurrentMap.Value.Position.Value = command.Position;

            return true;
        }
    }
}