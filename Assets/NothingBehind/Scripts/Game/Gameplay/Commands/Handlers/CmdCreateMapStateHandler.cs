using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers
{
    public class CmdCreateMapStateHandler : ICommandHandler<CmdCreateMapState>
    {
        private readonly GameStateProxy _gameState;
        private readonly GameSettings _gameSettings;

        public CmdCreateMapStateHandler(GameStateProxy gameState, GameSettings gameSettings)
        {
            _gameState = gameState;
            _gameSettings = gameSettings;
        }

        public bool Handle(CmdCreateMapState command)
        {
            var isMapAlreadyExisted = _gameState.Maps.Any(m => m.Id == command.MapId);

            if (isMapAlreadyExisted)
            {
                Debug.LogError($"Map with Id = {command.MapId} already exists");
                return false;
            }

            var newMapSettings = _gameSettings.MapsSettings.Maps.First(m => m.MapId == command.MapId);
            var newMapInitialStateSettings = newMapSettings.InitialStateSettings;

            var initialCharacters = new List<CharacterEntity>();
            foreach (var characterSettings in newMapInitialStateSettings.Characters)
            {
                var characterLevelSettings = characterSettings.LevelSettings;
                var initialCharacter = new CharacterEntity
                {
                    Id = _gameState.CreateEntityId(),
                    TypeId = characterSettings.TypeId,
                    Position = characterSettings.Position,
                    Level = characterLevelSettings.Level,
                    Health = characterLevelSettings.Health
                };

                initialCharacters.Add(initialCharacter);
            }

            var initialMapTransfers = new List<MapTransferData>();
            foreach (var mapTransferSettings in newMapInitialStateSettings.MapTransfers)
            {
                var mapTransferData = new MapTransferData(mapTransferSettings.TargetMapId,
                    mapTransferSettings.Position);
                initialMapTransfers.Add(mapTransferData);
            }

            var initialEnemySpawns = new List<EnemySpawnData>();
            foreach (var enemySpawnSettings in newMapInitialStateSettings.EnemySpawns)
            {
                var enemySpawnData = new EnemySpawnData(enemySpawnSettings.Id,
                    enemySpawnSettings.Characters, enemySpawnSettings.Position, enemySpawnSettings.IsTriggered);
                initialEnemySpawns.Add(enemySpawnData);
            }

            var sceneName = newMapSettings.SceneName;
            var newMapState = new MapState
            {
                Id = command.MapId,
                SceneName = sceneName,
                Characters = initialCharacters,
                MapTransfers = initialMapTransfers,
                EnemySpawns = initialEnemySpawns
            };

            var newMapStateProxy = new Map(newMapState);

            _gameState.Maps.Add(newMapStateProxy);

            return true;
        }
    }
}