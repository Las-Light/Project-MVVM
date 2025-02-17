using System;
using System.Collections.Generic;

namespace NothingBehind.Scripts.Game.State.Entities.Hero
{
    [Serializable]
    public class Hero : Entity
    {
        public string TypeId;
        public PositionOnMap CurrentMap;
        public List<PositionOnMap> PositionOnMaps;
        public float Health;
    }
}