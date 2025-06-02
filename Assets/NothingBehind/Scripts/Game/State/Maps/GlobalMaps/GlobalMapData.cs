using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Maps.GlobalMaps
{
    [Serializable]
    public class GlobalMapData
    {
        public MapId MapId;
        public string SceneName;
        public Vector3 PlayerInitialPosition;
        public List<MapTransferData> MapTransfers;
    }
}