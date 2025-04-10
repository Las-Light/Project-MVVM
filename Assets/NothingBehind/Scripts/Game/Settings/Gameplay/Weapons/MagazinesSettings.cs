using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "Magazines Settings", menuName = "Guns/Magazines Settings", order = 9)]
    public class MagazinesSettings:ScriptableObject
    {
        public string Caliber;
        public int ClipSize;
        public int CurrentAmmo;
    }
}