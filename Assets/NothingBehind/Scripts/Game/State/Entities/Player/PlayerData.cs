using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.State.Entities.Player
{
    [Serializable]
    public class PlayerData : Entity
    {
        public MapId CurrentMap;
        public List<PositionOnMap> PositionOnMaps;
        public float Health;
    }
}