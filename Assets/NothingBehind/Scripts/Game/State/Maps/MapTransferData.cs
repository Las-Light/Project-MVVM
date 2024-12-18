using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Maps
{
    [Serializable]
    public class MapTransferData
    {
        public MapTransferId MapTransferId;
        public Vector3Int Position;

        public MapTransferData(MapTransferId mapTransferId, Vector3Int position)
        {
            MapTransferId = mapTransferId;
            Position = position;
        }
    }
}