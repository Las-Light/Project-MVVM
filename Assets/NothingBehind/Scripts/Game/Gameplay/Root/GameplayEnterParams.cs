using NothingBehind.Scripts.Game.GameRoot;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public class GameplayEnterParams : SceneEnterParams
    {
        public string SaveFileName { get; }
        public int MapId { get; }

        public GameplayEnterParams(string saveFileName, int mapId) : base(Scenes.GAMEPLAY)
        {
            SaveFileName = saveFileName;
            MapId = mapId;
        }
    }
}