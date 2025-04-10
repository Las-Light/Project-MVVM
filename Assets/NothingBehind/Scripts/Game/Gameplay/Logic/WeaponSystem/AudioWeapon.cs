using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class AudioWeapon
    {
        public float Volume { get; set; }
        public AudioClip[] FireClips { get; set; }
        public AudioClip EmptyClip { get; set; }
        public AudioClip ReloadClip { get; set; }
        public AudioClip LastBulletClip { get; set; }

        public void PlayShootingClip(AudioSource AudioSource, bool IsLastBullet = false)
        {
            if (IsLastBullet && LastBulletClip != null)
            {
                AudioSource.PlayOneShot(LastBulletClip, Volume);
            }
            else
            {
                AudioSource.PlayOneShot(FireClips[Random.Range(0, FireClips.Length)], Volume);
            }
        }

        public void PlayOutOfAmmoClip(AudioSource AudioSource)
        {
            if (EmptyClip != null)
            {
                AudioSource.PlayOneShot(EmptyClip, Volume);
            }
        }

        public void PlayReloadClip(AudioSource AudioSource)
        {
            if (ReloadClip != null)
            {
                AudioSource.PlayOneShot(ReloadClip, Volume);
            }
        }
    }
}