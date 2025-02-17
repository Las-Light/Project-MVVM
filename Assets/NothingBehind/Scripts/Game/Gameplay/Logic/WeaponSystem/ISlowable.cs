using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.WeaponSystem
{
    public interface ISlowable
    {
        void Slow(AnimationCurve SlowCurve);
    }
}