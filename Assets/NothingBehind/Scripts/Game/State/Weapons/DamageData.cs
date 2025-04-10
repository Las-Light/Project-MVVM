using System;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    [Serializable]
    public class DamageData
    {
        public CurveType curveType;
        public float ConstantMax;
        public float ConstantMin;
        public float Constant;
        public float CurveMultiplier;
        public float Time;
        public float TimeMax;
        public float TimeMin;
        public float Value;
        public float ValueMax;
        public float ValueMin;
    }
}