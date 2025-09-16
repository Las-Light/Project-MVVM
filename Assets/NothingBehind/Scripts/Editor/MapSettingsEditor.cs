using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Markers;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities;
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
                mapSettings.InitialMapSettings = new MapInitialStateSettings(
                    GameObject.FindGameObjectWithTag("InitialPoint").transform.position,
                    FindObjectsByType<CharacterMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                        .Select(x => new EntityInitialStateSettings(
                            x.entity.EntityType,
                            x.entity.Level,
                            x.transform.position,
                            x.entity.ConfigId
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
                mapSettings.InitialMapSettings = null;
            }

            EditorUtility.SetDirty(target);
        }
    }
}