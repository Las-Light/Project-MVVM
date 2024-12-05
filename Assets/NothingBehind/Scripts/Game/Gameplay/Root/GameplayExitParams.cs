using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.MainMenu.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Root
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