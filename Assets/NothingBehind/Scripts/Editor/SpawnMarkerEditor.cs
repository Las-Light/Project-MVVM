using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Markers;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Maps;
using UnityEditor;
using UnityEngine;

namespace NothingBehind.Scripts.Editor
{
    [CustomEditor(typeof(SpawnMarker))]
    public class SpawnMarkerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SpawnMarker spawnMarker = (SpawnMarker)target;

            if (GUILayout.Button("Collect"))
            {
                spawnMarker.Id = spawnMarker.name;
                spawnMarker.Characters =
                    FindObjectsByType<CharacterMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                        .Select(x => new EntityInitialStateSettings(
                            x.entity.EntityType,
                            x.entity.Level,
                            x.transform.position,
                            x.entity.ConfigId
                        )).ToList();
                spawnMarker.Position = spawnMarker.transform.position;
            }

            if (GUILayout.Button("Clear"))
            {
                spawnMarker.Id = "";
                spawnMarker.Characters.Clear();
                spawnMarker.Position = Vector3.zero;
            }

            EditorUtility.SetDirty(target);
        }
    }
}