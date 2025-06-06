using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.WeaponSystem
{
    public interface ISlowable
    {
        void Slow(AnimationCurve SlowCurve);
    }
}