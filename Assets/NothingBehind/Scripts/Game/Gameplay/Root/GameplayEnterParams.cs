using NothingBehind.Scripts.Game.GameRoot;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public class GameplayEnterParams : SceneEnterParams
    {
        public string SaveFileName { get; }
        public string TargetSceneName { get; }
        public string TargetMapId { get; }

        public GameplayEnterParams(string saveFileName, string targetSceneName, string targetMapId) : base(targetSceneName, targetMapId)
        {
            SaveFileName = saveFileName;
            TargetSceneName = targetSceneName;
            TargetMapId = targetMapId;
        }
    }
}