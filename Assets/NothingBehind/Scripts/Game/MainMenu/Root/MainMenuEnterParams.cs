using NothingBehind.Scripts.Game.GameRoot;

namespace NothingBehind.Scripts.Game.MainMenu.Root
{
    public class MainMenuEnterParams : SceneEnterParams
    {
        public string TargetSceneName { get; }
        public string TargetMapId { get; }

        public MainMenuEnterParams(string targetSceneName, string targetMapId) : base(targetSceneName, targetMapId)
        {
            TargetSceneName = targetSceneName;
            TargetMapId = targetMapId;
        }
    }
}