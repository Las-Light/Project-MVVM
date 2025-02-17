using NothingBehind.Scripts.Game.Gameplay.Logic.WeaponSystem;
using NothingBehind.Scripts.Game.Gameplay.Logic.WeaponSystem.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapon
{
    [CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Config", order = 2)]
    public class ShootConfig : ScriptableObject
    {
        public bool IsHitscan = true;
        public Bullet BulletPrefab;
        public float BulletSpawnForce = 100;
        public LayerMask HitMask;
        public float FireRate = 0.25f;
        public int BulletsPerShot = 1;
        public BulletSpreadType SpreadType = BulletSpreadType.Simple;
        public float RecoilRecoverySpeed = 1f;
        public float MaxSpreadTime = 1f;
        public float BulletWeight = 0.1f;

        public ShootType ShootType = ShootType.FromGun;

        /// <summary>
        /// When <see cref="SpreadType"/> = <see cref="BulletSpreadType.Simple"/>, this value is used to compute the bullet spread.
        /// This defines the <b>Maximum</b> offset in any direction that a single shot can offset the current forward of the gun. 
        /// The range is from -x -> x, -y -> y, and -z -> z. 
        /// </summary>
        [Header("Simple Spread")] public Vector3 Spread = new Vector3(0.1f, 0.1f, 0.1f);

        public Vector3 MinSpread = Vector3.zero;

        [Header("Texture-Based Spread")]
        /// <summary>
        /// Multiplier applied to the vector from the center of <see cref="SpreadTexture"/> and the chosen pixel. 
        /// Smaller values mean less spread is applied.
        /// </summary>
        [Range(0.001f, 5f)]
        public float SpreadMultiplier = 0.1f;

        /// <summary>
        /// Weighted random values based on the Greyscale value of each pixel, originating from the center of the texture is used to calculate the spread offset.
        /// For more accurate guns, have strictly black pixels farther from the center of the image. 
        /// For very inaccurate weapons, you may choose to define grey/white values very far from the center 
        /// </summary>
        public Texture2D SpreadTexture;

    }
}