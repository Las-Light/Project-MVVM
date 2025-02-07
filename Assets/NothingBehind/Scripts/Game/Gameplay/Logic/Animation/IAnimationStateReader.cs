using NothingBehind.Scripts.Game.Gameplay.Logic.Hero;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Animation
{
    public interface IAnimationStateReader
    {
        public void EnteredState(int stateHash);
        public void ExitedState(int stateHash);
        AnimatorState State { get; }
    }
}