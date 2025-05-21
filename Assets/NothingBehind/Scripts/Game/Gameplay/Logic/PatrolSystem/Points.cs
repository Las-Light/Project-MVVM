using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem
{
    [Serializable]
    public class Points
    {
        public Vector3 PointPosition;
        public int Index;
        public int NextIndex;
        public bool HasBeenLinked;
        public bool HasBeenPassed;
        public float Height;
        public float DistToPoint;
        public Vector3 ShootPos;

        public void SetRandomPos(Vector3 pos, float range)
        {
            Vector2 randPosition = Random.insideUnitCircle * range;
            PointPosition = pos += new Vector3(randPosition.x, 0, randPosition.y);
        }

        public void SetSidePos(Transform centerTransform)
        {
            PointPosition = centerTransform.position + centerTransform.right * Random.Range(-2, 3);
        }
    }
}