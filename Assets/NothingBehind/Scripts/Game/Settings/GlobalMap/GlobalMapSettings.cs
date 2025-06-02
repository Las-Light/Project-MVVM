using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.GlobalMap
{
    [CreateAssetMenu(fileName = "GlobalMapSettings", menuName = "Game Settings/GlobalMap/New Global Map Settings", order = 0)]
    public class GlobalMapSettings : ScriptableObject
    {
        public MapId MapId;
        public string SceneName;
        public Vector3 PlayerInitialPosition;
        public List<MapTransferData> MapTransfers; 
    }
}