using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Maps.MapTransfer
{
    [Serializable]
    public class MapTransferData
    {
        public MapId TargetMapId;
        public Vector3 Position;

        public MapTransferData(MapId targetMapId, Vector3 position)
        {
            TargetMapId = targetMapId;
            Position = position;
        }
    }
}