using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class Knockback
    {
        public KnockbackData Origin { get; }
        public ReactiveProperty<float> KnockbackStrength { get; }
        public ReactiveProperty<float> MaximumKnockbackTime { get; }
        public ParticleSystem.MinMaxCurve DistanceFalloff { get; set; }

        public Knockback(KnockbackData data)
        {
            Origin = data;
            KnockbackStrength = new ReactiveProperty<float>(data.KnockbackStrength);
            MaximumKnockbackTime = new ReactiveProperty<float>(data.MaximumKnockbackTime);

            KnockbackStrength.Skip(1).Subscribe(value => data.KnockbackStrength = value);
            MaximumKnockbackTime.Skip(1).Subscribe(value => data.MaximumKnockbackTime = value);
        }
    }
}