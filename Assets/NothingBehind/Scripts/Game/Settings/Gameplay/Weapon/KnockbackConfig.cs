using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapon
{
    [CreateAssetMenu(fileName = "Knockback Config", menuName = "Guns/Knockback Config", order = 8)]
    public class KnockbackConfig : ScriptableObject
    {
        public float KnockbackStrength = 25000;
        public ParticleSystem.MinMaxCurve DistanceFalloff;
        public float MaximumKnockbackTime = 1;
    }
}