using System.Linq;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Weapons;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.MagazinesItems;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public static class WeaponDataFactory
    {
        public static WeaponData CreateWeaponData(GameState gameState, GameSettings gameSettings,
            int weaponId, WeaponName weaponName)
        {
            var weaponSettings = gameSettings.WeaponsSettings.WeaponConfigs.First(settings => settings.WeaponName == weaponName);
            var weaponData = new WeaponData
            {
                Id = weaponId,
                WeaponName = weaponSettings.WeaponName,
                ImpactType = weaponSettings.ImpactType,
                WeaponType = weaponSettings.WeaponType,
                Caliber = weaponSettings.Caliber,
                SpawnPoint = weaponSettings.SpawnPoint,
                SpawnRotation = weaponSettings.SpawnRotation,
                CheckDistanceToWall = weaponSettings.CheckDistanceToWall,
                AimingRange = weaponSettings.AimingRange,
                DamageData = CreateDamageData(weaponSettings.damageSettings),
                ShootData = CreateShootData(weaponSettings.shootSettings),
                FeedSystemData = CreateAmmoData(gameState, gameSettings),
                BulletPenetrationData = CreateBulletPenData(weaponSettings.bulletPenSettings),
                KnockbackData = CreateKnockbackData(weaponSettings.knockbackSettings),
            };

            return weaponData;
        }

        public static MagazinesData CreateMagazinesData(MagazinesSettings magazinesSettings)
        {
            var magazines = new MagazinesData
            {
                Caliber = magazinesSettings.Caliber,
                ClipSize = magazinesSettings.ClipSize,
                CurrentAmmo = magazinesSettings.CurrentAmmo
            };
            return magazines;
        }

        private static FeedSystemData CreateAmmoData(GameState gameState, GameSettings gameSettings)
        {
            var ammoData = ItemsDataFactory.CreateItemData(gameState, gameSettings, ItemType.Magazines);
            var weaponAmmoData = new FeedSystemData
            {
                MagazinesItemData = ammoData as MagazinesItemData
            };
            return weaponAmmoData;
        }

        private static KnockbackData CreateKnockbackData(KnockbackSettings knockbackSettings)
        {
            var knockbackData = new KnockbackData
            {
                KnockbackStrength = knockbackSettings.KnockbackStrength,
                MaximumKnockbackTime = knockbackSettings.MaximumKnockbackTime
            };
            return knockbackData;
        }

        private static BulletPenetrationData CreateBulletPenData(BulletPenetrationSettings bulletPenSettings)
        {
            var bulletPenData = new BulletPenetrationData
            {
                MaxObjectsToPenetrate = bulletPenSettings.MaxObjectsToPenetrate,
                MaxPenetrationDepth = bulletPenSettings.MaxPenetrationDepth,
                AccuracyLoss = bulletPenSettings.AccuracyLoss,
                DamageRetentionPercentage = bulletPenSettings.DamageRetentionPercentage
            };
            return bulletPenData;
        }

        private static ShootData CreateShootData(ShootSettings shootSettings)
        {
            var shootData = new ShootData
            {
                IsHitscan = shootSettings.IsHitscan,
                BulletSpawnForce = shootSettings.BulletSpawnForce,
                FireRate = shootSettings.FireRate,
                BulletsPerShot = shootSettings.BulletsPerShot,
                RecoilRecoverySpeed = shootSettings.RecoilRecoverySpeed,
                MaxSpreadTime = shootSettings.MaxSpreadTime,
                BulletWeight = shootSettings.BulletWeight,
                Spread = shootSettings.Spread,
                MinSpread = shootSettings.MinSpread,
                SpreadMultiplier = shootSettings.SpreadMultiplier,
                SpreadType = shootSettings.SpreadType,
                ShootType = shootSettings.ShootType,
                HitMask = shootSettings.HitMask,
            };
            return shootData;
        }

        private static DamageData CreateDamageData(DamageSettings damageSettings)
        {
            var damageCurve = damageSettings.DamageCurve;
            float timeMax = 0;
            float timeMin = 0;
            float time = 0;
            float valueMax = 0;
            float valueMin = 0;
            float value = 0;
            foreach (var keyframe in damageCurve.curveMax.keys)
            {
                timeMax = keyframe.time;
                valueMax = keyframe.value;
            }
            foreach (var keyframe in damageCurve.curveMin.keys)
            {
                timeMin = keyframe.time;
                valueMin = keyframe.value;
            }
            foreach (var keyframe in damageCurve.curve.keys)
            {
                time = keyframe.time;
                value = keyframe.value;
            }

            var damageData = new DamageData
            {
                curveType = damageSettings.curveType,
                ConstantMax = damageCurve.constantMax,
                ConstantMin = damageCurve.constantMin,
                Constant = damageCurve.constant,
                CurveMultiplier = damageCurve.curveMultiplier,
                Time = time,
                TimeMax = timeMax,
                TimeMin = timeMin,
                Value = value,
                ValueMax = valueMax,
                ValueMin = valueMin
            };
            return damageData;
        }
    }
}