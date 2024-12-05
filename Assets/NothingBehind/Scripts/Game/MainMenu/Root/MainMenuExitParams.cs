using NothingBehind.Scripts.Game.GameRoot;

namespace NothingBehind.Scripts.Game.MainMenu.Root
{
    public class MainMenuExitParams
    {
        public SceneEnterParams SceneEnterParams { get; }

        public MainMenuExitParams(SceneEnterParams sceneEnterParams)
        {
            SceneEnterParams = sceneEnterParams;
        }
    }
}