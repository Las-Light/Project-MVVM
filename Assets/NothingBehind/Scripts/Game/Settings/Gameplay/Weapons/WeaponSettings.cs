using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "Weapon Settings", menuName = "Weapons/Weapon Settings", order = 1)]
    public class WeaponSettings : ScriptableObject
    {
        public ImpactType ImpactType;
        public WeaponType WeaponType;
        public WeaponName WeaponName;
        public string Caliber;
        public GameObject ModelPrefab;
        public Vector3 SpawnPoint;
        public Vector3 SpawnRotation;
        public float CheckDistanceToWall;
        public float AimingRange;

        public DamageSettings damageSettings;
        public ShootSettings shootSettings;
        public FeedSystemSettings feedSystemSettings;
        public TrailSettings trailSettings;
        public AudioWeaponSettings audioSettings;
        public BulletPenetrationSettings bulletPenSettings;
        public KnockbackSettings knockbackSettings;
    }
}