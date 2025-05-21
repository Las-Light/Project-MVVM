using NothingBehind.Scripts.Game.GameRoot;

namespace NothingBehind.Scripts.Game.GlobalMap.Root
{
    public class GlobalMapExitParams
    {
        public SceneEnterParams SceneEnterParams { get; }

        public GlobalMapExitParams(SceneEnterParams sceneEnterParams)
        {
            SceneEnterParams = sceneEnterParams;
        }
    }
}