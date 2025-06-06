using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Root.Commands.PlayerCommands;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Root.Commands.Handlers.PlayerHandlers
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
            // var requiredMap = command.CurrentMapId;
            var requiredMap = _gameState.CurrentMapId.Value;
            var requiredPosOnMap = _gameState.Player.Value.PositionOnMaps.FirstOrDefault(
                r => r.MapId == requiredMap);
            
            if (requiredMap == MapId.Global_Map)
            {
                var playerInitialPosition =
                    _gameSettings.GlobalMapSettings.PlayerInitialPosition;
            
                if (requiredPosOnMap == null)
                {
                    CreateNewPosOnMap(requiredMap, playerInitialPosition);
                }
            }
            else
            {
                var initialStateSettings =
                    _gameSettings.MapsSettings.Maps.First(m => m.MapId == _gameState.CurrentMapId.Value).InitialStateSettings;
                var initialPosition = initialStateSettings.PlayerInitialPosition;
            
                if (requiredPosOnMap == null)
                {
                    CreateNewPosOnMap(requiredMap, initialPosition);
                }
            }

            _gameState.Player.Value.CurrentMapId.Value = requiredMap;
        }

        private void CreateNewPosOnMap(MapId requiredMap, Vector3 initialPosition)
        {
            var newPosOnMap = new PositionOnMapData()
            {
                MapId = requiredMap,
                Position = initialPosition
            };
            
            var posOnMap = new PositionOnMap(newPosOnMap);
            _gameState.Player.Value.PositionOnMaps.Add(posOnMap);
        }
    }
}