using NothingBehind.Scripts.Game.State.Root;
using R3;

namespace NothingBehind.Scripts.Game.State
{
    public interface IGameStateProvider
    {
        public GameStateProxy GameState { get; }

        public Observable<GameStateProxy> LoadGameState();
        public Observable<bool> SaveGameState();
        public Observable<bool> ResetGameState();
    }
}