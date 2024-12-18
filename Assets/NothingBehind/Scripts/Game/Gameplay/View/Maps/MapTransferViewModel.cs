using NothingBehind.Scripts.Game.State.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Maps
{
    public class MapTransferViewModel
    {
        public readonly MapTransferId MapTransferId;
        public readonly Vector3Int Position;

        public MapTransferViewModel(MapTransferData mapTransferData)
        {
            MapTransferId = mapTransferData.MapTransferId;
            Position = mapTransferData.Position;
        }
    }
}