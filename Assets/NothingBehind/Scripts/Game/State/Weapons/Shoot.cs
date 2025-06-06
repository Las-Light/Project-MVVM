using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.WeaponSystem;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class Shoot
    {
        public ShootData Origin { get; }
        public bool IsHitscan { get; }
        public Bullet BulletPrefab { get; set; }
        public ReactiveProperty<float> BulletSpawnForce { get; }
        public ReactiveProperty<float> FireRate { get; }
        public ReactiveProperty<int> BulletsPerShot { get; }
        public ReactiveProperty<float> RecoilRecoverySpeed { get; }
        public ReactiveProperty<float> MaxSpreadTime { get; }
        public ReactiveProperty<float> BulletWeight { get; }
        public ReactiveProperty<Vector3> Spread { get; }
        public ReactiveProperty<Vector3> MinSpread { get; }
        public ReactiveProperty<float> SpreadMultiplier { get; }
        public BulletSpreadType SpreadType { get; }
        public ShootType ShootType { get; }
        public LayerMask HitMask { get; }
        
        /// <summary>
        /// Weighted random values based on the Greyscale value of each pixel, originating from the center of the texture is used to calculate the spread offset.
        /// For more accurate guns, have strictly black pixels farther from the center of the image. 
        /// For very inaccurate weapons, you may choose to define grey/white values very far from the center 
        /// </summary>
        public Texture2D SpreadTexture { get; set; }

        public Shoot(ShootData data)
        {
            Origin = data;
            IsHitscan = data.IsHitscan;
            SpreadType = data.SpreadType;
            HitMask = data.HitMask;
            ShootType = data.ShootType;
            BulletSpawnForce = new ReactiveProperty<float>(data.BulletSpawnForce);
            FireRate = new ReactiveProperty<float>(data.FireRate);
            BulletsPerShot = new ReactiveProperty<int>(data.BulletsPerShot);
            RecoilRecoverySpeed = new ReactiveProperty<float>(data.RecoilRecoverySpeed);
            MaxSpreadTime = new ReactiveProperty<float>(data.MaxSpreadTime);
            BulletWeight = new ReactiveProperty<float>(data.BulletWeight);
            Spread = new ReactiveProperty<Vector3>(data.Spread);
            MinSpread = new ReactiveProperty<Vector3>(data.MinSpread);
            SpreadMultiplier = new ReactiveProperty<float>(data.SpreadMultiplier);

            BulletSpawnForce.Skip(1).Subscribe(value => data.BulletSpawnForce = value);
            FireRate.Skip(1).Subscribe(value => data.FireRate = value);
            BulletsPerShot.Skip(1).Subscribe(value => data.BulletsPerShot = value);
            RecoilRecoverySpeed.Skip(1).Subscribe(value => data.RecoilRecoverySpeed = value);
            MaxSpreadTime.Skip(1).Subscribe(value => data.MaxSpreadTime = value);
            BulletWeight.Skip(1).Subscribe(value => data.BulletWeight = value);
            Spread.Skip(1).Subscribe(value => data.Spread = value);
            MinSpread.Skip(1).Subscribe(value => data.MinSpread = value);
            SpreadMultiplier.Skip(1).Subscribe(value => data.SpreadMultiplier = value);
        }
        
        /**
        * Calculates and returns the offset from "forward" that should be applied for the bullet
        * based on <param name="ShootTime"/>. The closer to <see cref="MaxSpreadTime"/> this is, the
        * larger area of <see cref="SpreadTexture"/> is read, or wider range of <see cref="Spread"/>
        * is used, depending on <see cref="SpreadType"/>
        */
        public Vector3 GetSpread(float ShootTime = 0)
        {
            Vector3 spread = Vector3.zero;

            if (SpreadType == BulletSpreadType.Simple)
            {
                spread = Vector3.Lerp(
                    new Vector3(
                        Random.Range(-MinSpread.Value.x, MinSpread.Value.x),
                        Random.Range(-MinSpread.Value.y, MinSpread.Value.y),
                        Random.Range(-MinSpread.Value.z, MinSpread.Value.z)
                    ),
                    new Vector3(
                        Random.Range(-Spread.Value.x, Spread.Value.x),
                        Random.Range(-Spread.Value.y, Spread.Value.y),
                        Random.Range(-Spread.Value.z, Spread.Value.z)
                    ),
                    Mathf.Clamp01(ShootTime / MaxSpreadTime.Value)
                );
            }
            else if (SpreadType == BulletSpreadType.TextureBased)
            {
                spread = GetTextureDirection(ShootTime);
                spread *= SpreadMultiplier.Value;
            }

            return spread;
        }

        /// <summary>
        /// Reads provided <see cref="SpreadTexture"/> and uses a weighted random algorithm
        /// to determine the spread. <param name= "ShootTime" /> indicates how long the player
        /// has been shooting, larger values, closer to <see cref = "MaxSpreadTime" /> will sample
        /// larger areas of the texture
        /// </summary>         
        private Vector2 GetTextureDirection(float ShootTime)
        {
            Vector2 halfSize = new Vector2(SpreadTexture.width / 2f, SpreadTexture.height / 2f);

            int halfSquareExtents =
                Mathf.CeilToInt(Mathf.Lerp(0.01f, halfSize.x, Mathf.Clamp01(ShootTime / MaxSpreadTime.Value)));

            int minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtents;
            int minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtents;

            Color[] sampleColors = SpreadTexture.GetPixels(
                minX,
                minY,
                halfSquareExtents * 2,
                halfSquareExtents * 2
            );

            float[] colorsAsGrey = System.Array.ConvertAll(sampleColors, (color) => color.grayscale);
            float totalGreyValue = colorsAsGrey.Sum();

            float grey = Random.Range(0, totalGreyValue);
            int i = 0;
            for (; i < colorsAsGrey.Length; i++)
            {
                grey -= colorsAsGrey[i];
                if (grey <= 0)
                {
                    break;
                }
            }

            int x = minX + i % (halfSquareExtents * 2);
            int y = minY + i / (halfSquareExtents * 2);

            Vector2 targetPosition = new Vector2(x, y);

            Vector2 direction = (targetPosition - new Vector2(halfSize.x, halfSize.y)) / halfSize.x;

            return direction;
        }
    }
}