using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public class GameplayEnterParams : SceneEnterParams
    {
        public string SaveFileName { get; }
        public MapId TargetMapId { get; }

        public GameplayEnterParams(string saveFileName, MapId targetMapId) : base(targetMapId)
        {
            SaveFileName = saveFileName;
            TargetMapId = targetMapId;
        }
    }
}