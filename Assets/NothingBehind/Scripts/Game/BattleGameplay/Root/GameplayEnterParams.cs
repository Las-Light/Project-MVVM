using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.BattleGameplay.Root
{
    public class GameplayEnterParams : SceneEnterParams
    {
        public string SaveFileName { get; }

        public GameplayEnterParams(string saveFileName, MapId targetMapId) : base(targetMapId)
        {
            SaveFileName = saveFileName;
        }
    }
}