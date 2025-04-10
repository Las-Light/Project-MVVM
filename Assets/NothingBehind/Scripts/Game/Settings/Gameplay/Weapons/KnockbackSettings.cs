using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "Knockback Settings", menuName = "Guns/Knockback Settings", order = 8)]
    public class KnockbackSettings : ScriptableObject
    {
        public float KnockbackStrength = 25000;
        public ParticleSystem.MinMaxCurve DistanceFalloff;
        public float MaximumKnockbackTime = 1;
    }
}