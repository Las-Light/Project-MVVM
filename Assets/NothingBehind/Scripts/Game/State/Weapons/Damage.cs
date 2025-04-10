using System;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class Damage
    {
        public DamageData Origin { get; }
        public ParticleSystem.MinMaxCurve DamageCurve;
        public ReactiveProperty<CurveType> CurveType { get; }
        public ReactiveProperty<float> ConstantMax { get; }
        public ReactiveProperty<float> ConstantMin { get; }
        public ReactiveProperty<float> Constant { get; }
        public ReactiveProperty<float> CurveMultiplier { get; }
        public ReactiveProperty<float> Time { get; }
        public ReactiveProperty<float> TimeMax { get; }
        public ReactiveProperty<float> TimeMin { get; }
        public ReactiveProperty<float> Value { get; }
        public ReactiveProperty<float> ValueMax { get; }
        public ReactiveProperty<float> ValueMin { get; }

        public Damage(DamageData data)
        {
            Origin = data;
            CreateDamageCurve(data);
            CurveType = new ReactiveProperty<CurveType>(data.curveType);
            ConstantMax = new ReactiveProperty<float>(data.ConstantMax);
            ConstantMin = new ReactiveProperty<float>(data.ConstantMin);
            Constant = new ReactiveProperty<float>(data.Constant);
            CurveMultiplier = new ReactiveProperty<float>(data.CurveMultiplier);
            Time = new ReactiveProperty<float>(data.Time);
            TimeMax = new ReactiveProperty<float>(data.TimeMax);
            TimeMin = new ReactiveProperty<float>(data.TimeMin);
            Value = new ReactiveProperty<float>(data.Value);
            ValueMax = new ReactiveProperty<float>(data.ValueMax);
            ValueMin = new ReactiveProperty<float>(data.ValueMin);

            ConstantMax.Skip(1).Subscribe(value =>
            {
                DamageCurve.constantMax = value;
                data.ConstantMax = value;
            });
            ConstantMin.Skip(1).Subscribe(value =>
            {
                DamageCurve.constantMin = value;
                data.ConstantMin = value;
            });
            Constant.Skip(1).Subscribe(value =>
            {
                DamageCurve.constant = value;
                data.Constant = value;
            });
            CurveMultiplier.Skip(1).Subscribe(value =>
            {
                DamageCurve.curveMultiplier = value;
                data.CurveMultiplier = value;
            });
            CurveType.Skip(1).Subscribe(value => data.curveType = value);
            Time.Skip(1).Subscribe(value => data.Time = value);
            TimeMax.Skip(1).Subscribe(value => data.TimeMax = value);
            TimeMin.Skip(1).Subscribe(value => data.TimeMin = value);
            Value.Skip(1).Subscribe(value => data.Value = value);
            ValueMax.Skip(1).Subscribe(value => data.ValueMax = value);
            ValueMin.Skip(1).Subscribe(value => data.ValueMin = value);
        }

        private void CreateDamageCurve(DamageData data)
        {
            switch (data.curveType)
            {
                case TypeData.CurveType.TwoConstants:
                    DamageCurve = new ParticleSystem.MinMaxCurve(data.ConstantMin, data.ConstantMax);
                    break;
                case TypeData.CurveType.TwoCurves:
                    DamageCurve = new ParticleSystem.MinMaxCurve(data.CurveMultiplier,
                        CreateAnimationCurveMin(data.TimeMin, data.ValueMin),
                        CreateAnimationCurveMax(data.TimeMax, data.ValueMax));
                    break;
                case TypeData.CurveType.Curve:
                    DamageCurve = new ParticleSystem.MinMaxCurve(data.CurveMultiplier,
                        CreateAnimationCurve(data.Time, data.Value));
                    break;
                case TypeData.CurveType.Constant:
                    DamageCurve = new ParticleSystem.MinMaxCurve(data.Constant);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private AnimationCurve CreateAnimationCurveMax(float timeMax, float valueMax)
        {
            return new AnimationCurve(new Keyframe(timeMax, valueMax));
        }
        
        private AnimationCurve CreateAnimationCurveMin(float timeMin, float valueMin)
        {
            return new AnimationCurve(new Keyframe(timeMin, valueMin));
        }
        
        private AnimationCurve CreateAnimationCurve(float time, float value)
        {
            return new AnimationCurve(new Keyframe(time, value));
        }
    }
}