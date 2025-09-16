using System;
using NothingBehind.Scripts.Game.State.Maps.GameplayMaps;
using NothingBehind.Scripts.Game.State.Maps.GlobalMaps;

namespace NothingBehind.Scripts.Game.State.Maps
{
    public static class MapFactory
    {
        public static Map CreateMap(MapData mapData)
        {
            switch (mapData.MapType)
            {
                case MapType.GameplayMap:
                    return new GameplayMap(mapData as GameplayMapData);
                case MapType.GlobalMap:
                    return new GlobalMaps.GlobalMap(mapData as GlobalMapData);
                default:
                    throw new Exception("Unsupported map type" + mapData.MapType);
            }
        }
    }
}