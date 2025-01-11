using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State.Root;
using R3;

namespace NothingBehind.Scripts.Game.State
{
    public interface IGameStateProvider
    {
        public GameStateProxy GameState { get; }
        public GameSettingsStateProxy SettingsState { get; }

        public Observable<GameStateProxy> LoadGameState(GameSettings gameSettings, SceneEnterParams sceneEnterParams);
        public Observable<GameSettingsStateProxy> LoadSettingsState();
        public Observable<bool> SaveGameState();
        public Observable<bool> SaveSettingsState();
        public Observable<bool> ResetGameState(GameSettings gameSettings, SceneEnterParams sceneEnterParams);
        public Observable<GameSettingsStateProxy> ResetSettingsState();
    }
}