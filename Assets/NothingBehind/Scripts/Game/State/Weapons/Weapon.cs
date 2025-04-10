using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class Weapon
    {
        public WeaponData Origin { get; }
        public int Id { get; }
        public WeaponName WeaponName { get; }
        public ImpactType ImpactType { get; }
        public WeaponType WeaponType { get; }
        public string Caliber { get; }
        public ReactiveProperty<float> AimingRange { get; }
        public ReactiveProperty<float> CheckDistanceToWall { get; }
        public ReactiveProperty<Vector3> SpawnPoint { get; }
        public ReactiveProperty<Vector3> SpawnRotation { get; }
        public ReactiveProperty<Damage> Damage { get; }
        public ReactiveProperty<Shoot> Shoot { get; }
        public ReactiveProperty<FeedSystem> FeedSystem { get; }
        public ReactiveProperty<BulletPenetration> BulletPenetration { get; }
        public ReactiveProperty<Knockback> Knockback { get; }

        public Weapon(WeaponData data)
        {
            Origin = data;
            Id = data.Id;
            WeaponName = data.WeaponName;
            ImpactType = data.ImpactType;
            WeaponType = data.WeaponType;
            Caliber = data.Caliber;

            AimingRange = new ReactiveProperty<float>(data.AimingRange);
            CheckDistanceToWall = new ReactiveProperty<float>(data.CheckDistanceToWall);
            SpawnPoint = new ReactiveProperty<Vector3>(data.SpawnPoint);
            SpawnRotation = new ReactiveProperty<Vector3>(data.SpawnRotation);
            Damage = new ReactiveProperty<Damage>(new Damage(data.DamageData));
            Shoot = new ReactiveProperty<Shoot>(new Shoot(data.ShootData));
            FeedSystem = new ReactiveProperty<FeedSystem>(new FeedSystem(data.FeedSystemData));
            BulletPenetration =
                new ReactiveProperty<BulletPenetration>(new BulletPenetration(data.BulletPenetrationData));
            Knockback = new ReactiveProperty<Knockback>(new Knockback(data.KnockbackData));

            AimingRange.Skip(1).Subscribe(value => data.AimingRange = value);
            CheckDistanceToWall.Skip(1).Subscribe(value => data.CheckDistanceToWall = value);
            SpawnPoint.Skip(1).Subscribe(value => data.SpawnPoint = value);
            SpawnRotation.Skip(1).Subscribe(value => data.SpawnRotation = value);
            Damage.Skip(1).Subscribe(value => data.DamageData = value.Origin);
            Shoot.Skip(1).Subscribe(value => data.ShootData = value.Origin);
            FeedSystem.Skip(1).Subscribe(value => data.FeedSystemData = value.Origin);
            BulletPenetration.Skip(1).Subscribe(value => data.BulletPenetrationData = value.Origin);
            Knockback.Skip(1).Subscribe(value => data.KnockbackData = value.Origin);
        }
    }
}