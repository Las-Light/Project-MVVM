using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.MVVM.Transfers
{
    public class GameplayMapTransferViewModel
    {
        public readonly MapId MapId;
        public readonly Vector3 Position;

        public GameplayMapTransferViewModel(MapTransferData mapTransferData)
        {
            MapId = mapTransferData.TargetMapId;
            Position = mapTransferData.Position;
        }
    }
}