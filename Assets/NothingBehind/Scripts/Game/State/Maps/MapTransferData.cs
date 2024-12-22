using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Maps
{
    [Serializable]
    public class MapTransferData
    {
        public string SceneName;
        public MapId MapId;
        public Vector3Int Position;

        public MapTransferData(string sceneName, MapId mapId, Vector3Int position)
        {
            SceneName = sceneName;
            MapId = mapId;
            Position = position;
        }
    }
}