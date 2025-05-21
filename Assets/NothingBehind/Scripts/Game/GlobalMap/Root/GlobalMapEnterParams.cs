using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.GlobalMap.Root
{
    public class GlobalMapEnterParams: SceneEnterParams
    {
        public string SaveFileName { get; }
        
        public GlobalMapEnterParams(string saveFileName, MapId targetMapId) : base(targetMapId)
        {
            SaveFileName = saveFileName;
        }
    }
}