using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapon
{
    [CreateAssetMenu(fileName = "Damage Config", menuName = "Guns/Damage Config", order = 1)]
    public class DamageConfig : ScriptableObject
    {
        public ParticleSystem.MinMaxCurve DamageCurve;
    }
}