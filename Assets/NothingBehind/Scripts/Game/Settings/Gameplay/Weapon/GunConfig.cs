using NothingBehind.Scripts.Game.Gameplay.Logic.WeaponSystem.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapon
{
    [CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
    public class GunConfig : ScriptableObject
    {
        //public ImpactType ImpactType;
        public WeaponType Type;
        public string Name;
        public GameObject ModelPrefab;
        public Vector3 SpawnPoint;
        public Vector3 SpawnRotation;
        public float CheckDistanceToWall;
        public float AimingRange;

        public DamageConfig DamageConfig;
        public ShootConfig ShootConfig;
        public AmmoConfig AmmoConfig;
        public TrailConfig TrailConfig;
        public AudioWeaponConfig AudioConfig;
        public BulletPenetrationConfig BulletPenConfig;
        public KnockbackConfig KnockbackConfig;
    }
}