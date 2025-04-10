using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "Weapons Config", menuName = "Weapons Config/Weapons Config", order = 0)]
    public class WeaponsSettings : ScriptableObject
    {
        public List<WeaponSettings> WeaponConfigs;
    }
}