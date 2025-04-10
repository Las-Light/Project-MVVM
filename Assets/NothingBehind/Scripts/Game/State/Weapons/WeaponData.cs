using System;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    [Serializable]
    public class WeaponData
    {
        public int Id;
        public WeaponName WeaponName;
        public ImpactType ImpactType;
        public WeaponType WeaponType;
        public string Caliber;
        public Vector3 SpawnPoint;
        public Vector3 SpawnRotation;
        public float CheckDistanceToWall;
        public float AimingRange;

        public DamageData DamageData;
        public ShootData ShootData;
        public FeedSystemData FeedSystemData;
        public BulletPenetrationData BulletPenetrationData;
        public KnockbackData KnockbackData;
    }
}