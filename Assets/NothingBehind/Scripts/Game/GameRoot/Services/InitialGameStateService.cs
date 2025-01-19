using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Hero;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawn;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using NothingBehind.Scripts.Game.State.Root;

namespace NothingBehind.Scripts.Game.GameRoot.Services
{
    public class InitialGameStateService
    {
        public GameState CreateGameState(GameSettings gameSettings, SceneEnterParams sceneEnterParams)
        {
            var currentMapId = sceneEnterParams.TargetMapId;
            var currentMapSettings = gameSettings.MapsSettings.Maps.First(m => m.MapId == currentMapId);
            
            var gameState = new GameState();
            gameState.Maps = CreateMaps(gameState, gameSettings);
            gameState.CurrentMapId = currentMapId;
            gameState.Hero = new Hero()
            {
                CurrentMap = new PositionOnMap()
                {
                    MapId = currentMapId, Position = currentMapSettings.InitialStateSettings.PlayerInitialPosition
                },
                PositionOnMaps = new List<PositionOnMap>()
                {
                    new()
                    {
                        MapId = currentMapId, Position = currentMapSettings.InitialStateSettings.PlayerInitialPosition
                    }
                },
                Health = gameSettings.HeroSettings.Health
            };
            gameState.Resources = new List<ResourceData>()
            {
                new() { Amount = 0, ResourceType = ResourceType.SoftCurrency },
                new() { Amount = 0, ResourceType = ResourceType.HardCurrency }
            };

            return gameState;
        }

        private List<MapState> CreateMaps(GameState gameState, GameSettings gameSettings)
        {
            var maps = new List<MapState>();
            foreach (var map in gameSettings.MapsSettings.Maps)
            {
                maps.Add(CreateMapState(map.MapId, gameState, gameSettings));
            }
            return maps;
        }

        private MapState CreateMapState(MapId mapId, GameState gameState, GameSettings gameSettings)
        {
            var newMapSettings = gameSettings.MapsSettings.Maps.First(m => m.MapId == mapId);
            var newMapInitialStateSettings = newMapSettings.InitialStateSettings;
            
            var sceneName = newMapSettings.SceneName;
            var newMapState = new MapState
            {
                Id = mapId,
                SceneName = sceneName,
                Characters = InitialCharacters(newMapInitialStateSettings, gameState),
                MapTransfers = InitialMapTransfers(newMapInitialStateSettings),
                EnemySpawns = InitialEnemySpawns(newMapInitialStateSettings)
            };

            return newMapState;
        }

        private List<EnemySpawnData> InitialEnemySpawns(MapInitialStateSettings newMapInitialStateSettings)
        {
            var initialEnemySpawns = new List<EnemySpawnData>();
            foreach (var enemySpawnSettings in newMapInitialStateSettings.EnemySpawns)
            {
                var enemySpawnData = new EnemySpawnData(enemySpawnSettings.Id,
                    enemySpawnSettings.Characters, enemySpawnSettings.Position, enemySpawnSettings.IsTriggered);
                initialEnemySpawns.Add(enemySpawnData);
            }

            return initialEnemySpawns;
        }

        private List<MapTransferData> InitialMapTransfers(MapInitialStateSettings newMapInitialStateSettings)
        {
            var initialMapTransfers = new List<MapTransferData>();
            foreach (var mapTransferSettings in newMapInitialStateSettings.MapTransfers)
            {
                var mapTransferData = new MapTransferData(mapTransferSettings.TargetMapId,
                    mapTransferSettings.Position);
                initialMapTransfers.Add(mapTransferData);
            }

            return initialMapTransfers;
        }

        private List<CharacterEntity> InitialCharacters(MapInitialStateSettings newMapInitialStateSettings,
            GameState gameState)
        {
            var initialCharacters = new List<CharacterEntity>();
            foreach (var characterSettings in newMapInitialStateSettings.Characters)
            {
                var characterLevelSettings = characterSettings.LevelSettings;
                var initialCharacter = new CharacterEntity
                {
                    Id = gameState.CreateEntityId(),
                    TypeId = characterSettings.TypeId,
                    Position = characterSettings.Position,
                    Level = characterLevelSettings.Level,
                    Health = characterLevelSettings.Health
                };

                initialCharacters.Add(initialCharacter);
            }

            return initialCharacters;
        }
    }
}