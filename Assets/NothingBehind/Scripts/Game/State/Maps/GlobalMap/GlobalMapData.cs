using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;

namespace NothingBehind.Scripts.Game.State.Maps.GlobalMap
{
    [Serializable]
    public class GlobalMapData
    {
        public MapId Id;
        public string SceneName;
        public List<MapTransferData> MapTransfers;
    }
}