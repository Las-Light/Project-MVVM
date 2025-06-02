using System.Linq;
using NothingBehind.Scripts.Game.GlobalMap.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.GlobalMap;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.GlobalMap.Commands.Handlers.PlayerHandlers
{
    public class CmdInitPlayerPosOnGlobalMapHandler: ICommandHandler<CmdInitPlayerPosOnGlobalMap>
    {
        private readonly GameStateProxy _gameState;
        private readonly GameSettings _gameSettings;

        public CmdInitPlayerPosOnGlobalMapHandler(GameStateProxy gameState, GameSettings gameSettings)
        {
            _gameState = gameState;
            _gameSettings = gameSettings;
        }
        public CommandResult Handle(CmdInitPlayerPosOnGlobalMap command)
        {
            InitialPlayerPosition(command);
            return new CommandResult( true);
        }
        
        private void InitialPlayerPosition(CmdInitPlayerPosOnGlobalMap command)
        {
            var requiredMap = command.CurrentMapId;
            var requiredPosOnMap = _gameState.Player.Value.PositionOnMaps.FirstOrDefault(
                r => r.MapId == requiredMap);
            var globalMapSettings =
                _gameSettings.GlobalMapSettings;
            
            if (requiredPosOnMap == null)
            {
                CreateNewPosOnMap(requiredMap, globalMapSettings);
            }

            _gameState.Player.Value.CurrentMapId.Value = requiredMap;
        }

        private void CreateNewPosOnMap(MapId requiredMap, GlobalMapSettings globalMapSettings)
        {
            var newPosOnMap = new PositionOnMapData()
            {
                MapId = requiredMap,
                Position = globalMapSettings.PlayerInitialPosition
            };
            
            var posOnMap = new PositionOnMap(newPosOnMap);
            _gameState.Player.Value.PositionOnMaps.Add(posOnMap);
        }
    }
}