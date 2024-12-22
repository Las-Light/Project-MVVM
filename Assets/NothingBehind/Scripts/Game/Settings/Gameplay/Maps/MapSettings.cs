using NothingBehind.Scripts.Game.State.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Maps
{
    [CreateAssetMenu(fileName = "MapSettings", menuName = "Game Settings/Maps/New Map Settings")]
    public class MapSettings : ScriptableObject
    {
        public MapId MapId;
        public string SceneName;
        public MapInitialStateSettings InitialStateSettings;
    }
}