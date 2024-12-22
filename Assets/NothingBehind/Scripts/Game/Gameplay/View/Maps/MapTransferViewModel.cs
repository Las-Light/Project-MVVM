using NothingBehind.Scripts.Game.State.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Maps
{
    public class MapTransferViewModel
    {
        public readonly MapId MapId;
        public readonly Vector3Int Position;

        public MapTransferViewModel(MapTransferData mapTransferData)
        {
            MapId = mapTransferData.MapId;
            Position = mapTransferData.Position;
        }
    }
}