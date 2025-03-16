using NothingBehind.Scripts.Game.State.Maps;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Player
{
    public class PositionOnMapProxy
    {
        public MapId MapId => Origin.MapId;
        public ReactiveProperty<Vector3> Position { get; }

        public PositionOnMap Origin { get; }

        public PositionOnMapProxy(PositionOnMap positionOnMap)
        {
            Origin = positionOnMap;
            Position = new ReactiveProperty<Vector3>(positionOnMap.Position);

            Position.Skip(1).Subscribe(value => positionOnMap.Position = value);
        }
    }
}