using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Markers;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Maps;
using UnityEditor;
using UnityEngine;

namespace NothingBehind.Scripts.Editor
{
    [CustomEditor(typeof(MapSettings))]
    public class MapSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            MapSettings mapSettings = (MapSettings)target;

            if (GUILayout.Button("Collect"))
            {
                mapSettings.InitialStateSettings = new MapInitialStateSettings(
                    FindObjectsByType<CharacterMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                        .Select(x => new CharacterInitialStateSettings(
                            x.Character.TypeId,
                            x.Character.LevelSettings,
                            x.transform.position
                        )).ToList(),
                    FindObjectsByType<MapTransferMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                        .Select(x => new MapTransferData(x.TargetMapId, x.transform.position)).ToList(),
                    FindObjectsByType<SpawnMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                        .Select(x => x.EnemySpawn)
                        .ToList());
            }

            EditorUtility.SetDirty(target);
        }
    }
}