using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GlobalMap.MVVM.Maps
{
    public class GlobalMapTransferViewModel
    {
        public readonly MapId MapId;
        public readonly Vector3 Position;
        public GlobalMapTransferViewModel(MapTransferData data)
        {
            MapId = data.TargetMapId;
            Position = data.Position;
        }
    }
}