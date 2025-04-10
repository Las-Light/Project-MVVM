using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "Ammo Settings", menuName = "Guns/Ammo Settings", order = 4)]
    public class AmmoSettings : ScriptableObject
    {
        public MagazinesSettings MagazinesSettings;
    }
}