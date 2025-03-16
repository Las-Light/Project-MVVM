using System;
using NothingBehind.Scripts.Game.State.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Player
{
    [Serializable]
    public class PositionOnMap
    {
        public MapId MapId;
        public Vector3 Position;
    }
}