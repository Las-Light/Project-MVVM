using System;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    [Serializable]
    public class ShootData
    {
        public bool IsHitscan;
        public float BulletSpawnForce;
        public float FireRate;
        public int BulletsPerShot;
        public float RecoilRecoverySpeed;
        public float MaxSpreadTime;
        public float BulletWeight;
        public Vector3 Spread;
        public Vector3 MinSpread;
        public float SpreadMultiplier;
        public BulletSpreadType SpreadType;
        public ShootType ShootType;
        public LayerMask HitMask;
    }
}