using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.PlayerHandlers
{
    public class CmdInitPlayerPosOnMapHandler : ICommandHandler<CmdInitPlayerPosOnMap>
    {
        private readonly GameStateProxy _gameState;
        private readonly GameSettings _gameSettings;

        public CmdInitPlayerPosOnMapHandler(GameStateProxy gameState, GameSettings gameSettings)
        {
            _gameState = gameState;
            _gameSettings = gameSettings;
        }
        public CommandResult Handle(CmdInitPlayerPosOnMap command)
        {
            InitialPlayerPosition(command);
            return new CommandResult( true);
        }
        
        private void InitialPlayerPosition(CmdInitPlayerPosOnMap command)
        {
            var requiredMap = command.CurrentMapId;
            var requiredPosOnMap = _gameState.Player.Value.PositionOnMaps.FirstOrDefault(
                r => r.MapId == requiredMap);
            var initialStateSettings =
                _gameSettings.MapsSettings.Maps.First(m => m.MapId == command.CurrentMapId).InitialStateSettings;
            
            if (requiredPosOnMap == null)
            {
                CreateNewPosOnMap(requiredMap, initialStateSettings);
            }

            _gameState.Player.Value.CurrentMapId.Value = requiredMap;
        }

        private void CreateNewPosOnMap(MapId requiredMap, MapInitialStateSettings initialStateSettings)
        {
            var newPosOnMap = new PositionOnMapData()
            {
                MapId = requiredMap,
                Position = initialStateSettings.PlayerInitialPosition
            };
            
            var posOnMap = new PositionOnMap(newPosOnMap);
            _gameState.Player.Value.PositionOnMaps.Add(posOnMap);
        }
    }
}