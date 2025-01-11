using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.State.Entities.Hero
{
    [Serializable]
    public class Hero
    {
        public MapId CurrentMap; 
        public List<PositionOnMap> PositionOnMaps;
        public float Health;
    }
}