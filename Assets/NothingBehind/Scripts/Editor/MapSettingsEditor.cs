using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Markers;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                mapSettings.SceneName = SceneManager.GetActiveScene().name;
                mapSettings.InitialStateSettings = new MapInitialStateSettings(
                    GameObject.FindGameObjectWithTag("InitialPoint").transform.position,
                    FindObjectsByType<CharacterMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                        .Select(x => new CharacterInitialStateSettings(
                            x.Character.EntityType,
                            x.Character.LevelSettings,
                            x.transform.position
                        )).ToList(),
                    FindObjectsByType<MapTransferMarker>(FindObjectsInactive.Exclude,
                            FindObjectsSortMode.None)
                        .Select(x=> new MapTransferData(x.TargetMapId, x.transform.position))
                        .ToList(),
                    FindObjectsByType<SpawnMarker>(FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None)
                        .Select(x=> new EnemySpawnData(x.Id, x.Characters, x.Position, x.IsTriggered))
                        .ToList()
                    );
            }
            
            if (GUILayout.Button("Clear"))
            {
                mapSettings.SceneName = "";
                mapSettings.InitialStateSettings = null;
            }

            EditorUtility.SetDirty(target);
        }
    }
}