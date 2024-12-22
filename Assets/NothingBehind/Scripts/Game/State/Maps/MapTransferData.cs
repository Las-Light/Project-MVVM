using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Maps
{
    [Serializable]
    public class MapTransferData
    {
        public MapId MapId;
        public Vector3Int Position;

        public MapTransferData(MapId mapId, Vector3Int position)
        {
            MapId = mapId;
            Position = position;
        }
    }
}