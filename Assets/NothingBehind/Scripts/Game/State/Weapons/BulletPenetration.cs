using R3;
using Unity.VisualScripting;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class BulletPenetration
    {
        public BulletPenetrationData Origin { get; }
        public ReactiveProperty<int> MaxObjectsToPenetrate;
        public ReactiveProperty<float> MaxPenetrationDepth;
        public ReactiveProperty<Vector3> AccuracyLoss;
        public ReactiveProperty<float> DamageRetentionPercentage;

        public BulletPenetration(BulletPenetrationData data)
        {
            Origin = data;
            MaxObjectsToPenetrate = new ReactiveProperty<int>(data.MaxObjectsToPenetrate);
            MaxPenetrationDepth = new ReactiveProperty<float>(data.MaxPenetrationDepth);
            AccuracyLoss = new ReactiveProperty<Vector3>(data.AccuracyLoss);
            DamageRetentionPercentage = new ReactiveProperty<float>(data.DamageRetentionPercentage);

            MaxObjectsToPenetrate.Skip(1).Subscribe(value => data.MaxObjectsToPenetrate = value);
            MaxPenetrationDepth.Skip(1).Subscribe(value => data.MaxPenetrationDepth = value);
            AccuracyLoss.Skip(1).Subscribe(value => data.AccuracyLoss = value);
            DamageRetentionPercentage.Skip(1).Subscribe(value => data.DamageRetentionPercentage = value);
        }
    }
}