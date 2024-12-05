using NothingBehind.Scripts.Game.GameRoot;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public class GameplayEnterParams : SceneEnterParams
    {
        public string SaveFileName { get; }
        public string MapId { get; }

        public GameplayEnterParams(string saveFileName, string mapId) : base(mapId)
        {
            SaveFileName = saveFileName;
            MapId = mapId;
        }
    }
}