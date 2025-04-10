using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "Bullet Penetration Settings", menuName = "Guns/Bullet Penetration Settings", order = 7)]
    public class BulletPenetrationSettings : ScriptableObject
    {
        public int MaxObjectsToPenetrate = 0;
        public float MaxPenetrationDepth = 0.275f;
        public Vector3 AccuracyLoss = new(0.1f, 0.1f, 0.1f);
        public float DamageRetentionPercentage;
    }
}