using NothingBehind.Scripts.Game.GameRoot;

namespace NothingBehind.Scripts.Game.BattleGameplay.Root
{
    public class GameplayExitParams
    {
        public SceneEnterParams SceneEnterParams { get; }
        public GameplayExitParams(SceneEnterParams sceneEnterParams)
        {
            SceneEnterParams = sceneEnterParams;
        }
    }
}