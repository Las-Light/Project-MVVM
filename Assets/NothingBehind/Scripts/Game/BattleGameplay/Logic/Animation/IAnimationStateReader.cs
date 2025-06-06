namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.Animation
{
    public interface IAnimationStateReader
    {
        public void EnteredState(int stateHash);
        public void ExitedState(int stateHash);
        AnimatorState State { get; }
    }
}