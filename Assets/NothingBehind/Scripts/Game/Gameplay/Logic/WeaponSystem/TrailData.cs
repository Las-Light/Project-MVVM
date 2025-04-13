using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.WeaponSystem
{
    public class TrailData
    {
        public Material Material;
        public AnimationCurve WidthCurve;
        public float Duration;
        public float MinVertexDistance;
        public Gradient Color;

        public float MissDistance;
        public float SimulationSpeed;
    }
}