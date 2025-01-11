using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Maps
{
    public class MapTransferViewModel
    {
        public readonly MapId MapId;
        public readonly Vector3 Position;

        public MapTransferViewModel(MapTransferData mapTransferData)
        {
            MapId = mapTransferData.TargetMapId;
            Position = mapTransferData.Position;
        }
    }
}