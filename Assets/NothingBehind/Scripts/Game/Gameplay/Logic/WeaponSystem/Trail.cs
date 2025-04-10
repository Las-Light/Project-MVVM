using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class Trail
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