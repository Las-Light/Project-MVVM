using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapon
{
    [CreateAssetMenu(fileName = "Ammo Config", menuName = "Guns/Ammo Config", order = 3)]
    public class AmmoConfig : ScriptableObject
    {
        public int MaxAmmo = 120;
        public int ClipSize = 30;

        public int CurrentAmmo = 120;
        public int CurrentClipAmmo = 30;
    }
}