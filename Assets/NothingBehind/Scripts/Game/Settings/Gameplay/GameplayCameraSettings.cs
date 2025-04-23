using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayCameraSettings", menuName = "Game Settings/Camera/New Gameplay Camera Settings")]
    public class GameplayCameraSettings : ScriptableObject
    {
        public float RotationSpeed = 60f;
        [Range(0.01f, 5.0f)] public float FollowSpeed = 0.5f;
    }
}