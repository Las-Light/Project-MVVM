namespace NothingBehind.Scripts.Game.GameRoot
{
    public class SceneEnterParams
    {
        public string MapId { get; }

        public SceneEnterParams(string mapId)
        {
            MapId = mapId;
        }

        public T As<T>() where T : SceneEnterParams
        {
            return (T)this;
        }
    }
}