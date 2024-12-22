namespace NothingBehind.Scripts.Game.GameRoot
{
    public class SceneEnterParams
    {
        public string TargetSceneName { get; }
        public string TargetMapId { get; }

        public SceneEnterParams(string targetSceneName, string targetMapId)
        {
            TargetSceneName = targetSceneName;
            TargetMapId = targetMapId;
        }

        public T As<T>() where T : SceneEnterParams
        {
            return (T)this;
        }
    }
}