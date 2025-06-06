using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Markers;
using NothingBehind.Scripts.Game.Settings.GlobalMap;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NothingBehind.Scripts.Editor
{
    [CustomEditor(typeof(GlobalMapSettings))]
    public class GlobalMapSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GlobalMapSettings mapSettings = (GlobalMapSettings)target;

            if (GUILayout.Button("Collect"))
            {
                mapSettings.SceneName = SceneManager.GetActiveScene().name;
                mapSettings.PlayerInitialPosition =
                    GameObject.FindGameObjectWithTag("InitialPoint").transform.position;
                mapSettings.MapTransfers =
                    FindObjectsByType<MapTransferMarker>(FindObjectsInactive.Exclude,
                            FindObjectsSortMode.None)
                        .Select(x => new MapTransferData(x.TargetMapId, x.transform.position))
                        .ToList();
            }

            if (GUILayout.Button("Clear"))
            {
                mapSettings.SceneName = "";
                mapSettings.PlayerInitialPosition = Vector3.zero;
                mapSettings.MapTransfers.Clear();
            }

            EditorUtility.SetDirty(target);
        }
    }
}