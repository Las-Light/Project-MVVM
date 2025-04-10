using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "Damage Settings", menuName = "Guns/Damage Settings", order = 2)]
    public class DamageSettings : ScriptableObject
    {
        public CurveType curveType;
        public ParticleSystem.MinMaxCurve DamageCurve;
    }
}