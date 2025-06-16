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

            if (GUILayout.Button("CollectSceneName"))
            {
                mapSettings.SceneName = SceneManager.GetActiveScene().name;
            }

            if (GUILayout.Button("CollectPlayerInitialPosition"))
            {
                mapSettings.PlayerInitialPosition =
                    GameObject.FindGameObjectWithTag("InitialPoint").transform.position;
            }
            
            if (GUILayout.Button("CollectMapTransfers"))
            {
                mapSettings.MapTransfers =
                    FindObjectsByType<MapTransferMarker>(FindObjectsInactive.Exclude,
                            FindObjectsSortMode.None)
                        .Select(x => new MapTransferData(x.TargetMapId, x.transform.position))
                        .ToList();
            }
            
            if (GUILayout.Button("ClearSceneName"))
            {
                mapSettings.SceneName = "";
            }
            
            if (GUILayout.Button("ClearPlayerInitialPosition"))
            {
                mapSettings.PlayerInitialPosition = Vector3.zero;
            }
            
            if (GUILayout.Button("ClearMapTransfers"))
            {
                mapSettings.MapTransfers.Clear();
            }

            if (GUILayout.Button("ClearAll"))
            {
                mapSettings.SceneName = "";
                mapSettings.PlayerInitialPosition = Vector3.zero;
                mapSettings.MapTransfers.Clear();
            }

            EditorUtility.SetDirty(target);
        }
    }
}