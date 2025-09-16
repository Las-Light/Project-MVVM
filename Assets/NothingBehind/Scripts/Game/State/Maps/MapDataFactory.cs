using System;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Maps.GameplayMaps;
using NothingBehind.Scripts.Game.State.Maps.GlobalMaps;

namespace NothingBehind.Scripts.Game.State.Maps
{
    public static class MapDataFactory
    {
        public static MapData CreateMapData(
            MapInitialStateSettings mapInitialStateSettings,
            MapsSettings mapsSettings)
        {
            switch (mapInitialStateSettings.MapType)
            {
                case MapType.GameplayMap:
                    return CreateMapData<GameplayMapData>(mapInitialStateSettings, mapsSettings);
                case MapType.GlobalMap:
                    return CreateMapData<GlobalMapData>(mapInitialStateSettings, mapsSettings);
                default:
                    throw new Exception($"Not implemented map creation: {mapInitialStateSettings.MapType}");
            }
        }
        
        private static T CreateMapData<T>(MapInitialStateSettings initialSettings, MapsSettings mapsSettings) where T : MapData, new()
        {
            return CreateMapData<T>(
                initialSettings.MapType,
                mapsSettings);
        }

        public static T CreateMapData<T>(
            MapType mapType,
            MapsSettings mapsSettings)
            where T : MapData, new()
        {
            var map = new T
            {
                MapType = mapType,
            };

            switch (map)
            {
                case GameplayMapData gameplayMapData:
                    UpdateGameplayMap(gameplayMapData, mapsSettings);
                    break;
                
                case GlobalMapData globalMapData:
                    UpdateGlobalMap(globalMapData, mapsSettings);
                    break;
                
                default:
                    throw new Exception($"Not implemented map creation: {mapType}");
            }

            return map;
        }
        
        private static void UpdateGameplayMap(GameplayMapData gameplayMapData, MapsSettings mapsSettings)
        {
        }
        
        private static void UpdateGlobalMap(GlobalMapData globalMapData, MapsSettings mapsSettings)
        {
        }
    }
}