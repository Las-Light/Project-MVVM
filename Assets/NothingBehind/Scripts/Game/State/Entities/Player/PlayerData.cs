using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.State.Entities.Player
{
    [Serializable]
    public class PlayerData : Entity
    {
        public MapId CurrentMapId;
        public List<PositionOnMapData> PositionOnMaps;
        public float Health;
    }
}