using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    [Serializable]
    public class BulletPenetrationData
    {
        public int MaxObjectsToPenetrate;
        public float MaxPenetrationDepth;
        public Vector3 AccuracyLoss;
        public float DamageRetentionPercentage;
    }
}