using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.GameRoot
{
    public class SceneEnterParams
    {
        public MapId TargetMapId { get; }

        public SceneEnterParams(MapId targetMapId)
        {
            TargetMapId = targetMapId;
        }

        public T As<T>() where T : SceneEnterParams
        {
            return (T)this;
        }
    }
}