using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.Hero;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Hero;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Hero
{
    public class CmdInitHeroPosOnMapHandler : ICommandHandler<CmdInitPlayerPosOnMap>
    {
        private readonly GameStateProxy _gameState;
        private readonly GameSettings _gameSettings;

        public CmdInitHeroPosOnMapHandler(GameStateProxy gameState, GameSettings gameSettings)
        {
            _gameState = gameState;
            _gameSettings = gameSettings;
        }
        public bool Handle(CmdInitPlayerPosOnMap command)
        {
            InitialHeroPosition(command);
            return true;
        }
        
        private void InitialHeroPosition(CmdInitPlayerPosOnMap command)
        {
            var requiredMap = command.CurrentMapId;
            var requiredPosOnMap = _gameState.Hero.Value.PositionOnMaps.FirstOrDefault(
                r => r.MapId == requiredMap);
            var initialStateSettings =
                _gameSettings.MapsSettings.Maps.First(m => m.MapId == command.CurrentMapId).InitialStateSettings;
            
            if (requiredPosOnMap == null)
            {
                requiredPosOnMap = CreateNewPosOnMap(requiredMap, initialStateSettings);
            }

            _gameState.Hero.Value.CurrentMap.Value = requiredPosOnMap;
        }

        private PositionOnMapProxy CreateNewPosOnMap(MapId requiredMap, MapInitialStateSettings initialStateSettings)
        {
            var newPosOnMap = new PositionOnMap()
            {
                MapId = requiredMap,
                Position = initialStateSettings.PlayerInitialPosition
            };
            
            var posOnMap = new PositionOnMapProxy(newPosOnMap);
            _gameState.Hero.Value.PositionOnMaps.Add(posOnMap);
            _gameState.Hero.Value.CurrentMap.Value = posOnMap;
            return posOnMap;
        }
    }
}