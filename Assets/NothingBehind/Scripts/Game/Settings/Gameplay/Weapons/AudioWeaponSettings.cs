using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "Audio Settings", menuName = "Guns/Audio Settings", order = 6)]
    public class AudioWeaponSettings : ScriptableObject
    {
        [Range(0, 1f)]
        public float Volume = 1f;
        public AudioClip[] FireClips;
        public AudioClip EmptyClip;
        public AudioClip ReloadClip;
        public AudioClip LastBulletClip;
    }
}