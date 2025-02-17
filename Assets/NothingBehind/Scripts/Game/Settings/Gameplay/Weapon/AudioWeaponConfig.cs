using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapon
{
    [CreateAssetMenu(fileName = "Audio Config", menuName = "Guns/Audio Config", order = 5)]
    public class AudioWeaponConfig : ScriptableObject
    {
        [Range(0, 1f)]
        public float Volume = 1f;
        public AudioClip[] FireClips;
        public AudioClip EmptyClip;
        public AudioClip ReloadClip;
        public AudioClip LastBulletClip;
    }
}