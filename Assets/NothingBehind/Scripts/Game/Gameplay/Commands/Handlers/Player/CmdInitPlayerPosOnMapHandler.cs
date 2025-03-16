using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.Player;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Player
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
        public bool Handle(CmdInitPlayerPosOnMap command)
        {
            InitialPlayerPosition(command);
            return true;
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

            _gameState.Player.Value.CurrentMap.Value = requiredMap;
        }

        private void CreateNewPosOnMap(MapId requiredMap, MapInitialStateSettings initialStateSettings)
        {
            var newPosOnMap = new PositionOnMap()
            {
                MapId = requiredMap,
                Position = initialStateSettings.PlayerInitialPosition
            };
            
            var posOnMap = new PositionOnMapProxy(newPosOnMap);
            _gameState.Player.Value.PositionOnMaps.Add(posOnMap);
        }
    }
}