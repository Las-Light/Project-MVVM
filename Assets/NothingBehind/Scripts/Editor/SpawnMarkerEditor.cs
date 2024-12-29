using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Markers;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
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
                spawnMarker.EnemySpawn = new EnemySpawnData(
                    spawnMarker.Id,
                    FindObjectsByType<CharacterMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                        .Select(x => new CharacterInitialStateSettings(
                            x.Character.TypeId,
                            x.Character.LevelSettings,
                            x.transform.position
                        )).ToList(),
                    spawnMarker.transform.position);
            }

            if (GUILayout.Button("Clear")) spawnMarker.EnemySpawn.Characters.Clear();

            EditorUtility.SetDirty(target);
        }
    }
}