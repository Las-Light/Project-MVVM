using NothingBehind.Scripts.Game.State.Maps;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Player
{
    public class PositionOnMap
    {
        public MapId MapId => Origin.MapId;
        public ReactiveProperty<Vector3> Position { get; }

        public PositionOnMapData Origin { get; }

        public PositionOnMap(PositionOnMapData positionOnMapData)
        {
            Origin = positionOnMapData;
            Position = new ReactiveProperty<Vector3>(positionOnMapData.Position);

            Position.Skip(1).Subscribe(value => positionOnMapData.Position = value);
        }
    }
}