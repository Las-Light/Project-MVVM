using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapon
{
    [CreateAssetMenu(fileName = "Bullet Penetration Config", menuName = "Guns/Bullet Penetration Config", order = 6)]
    public class BulletPenetrationConfig : ScriptableObject
    {
        public int MaxObjectsToPenetrate = 0;
        public float MaxPenetrationDepth = 0.275f;
        public Vector3 AccuracyLoss = new(0.1f, 0.1f, 0.1f);
        public float DamageRetentionPercentage;
    }
}