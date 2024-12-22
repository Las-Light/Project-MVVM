using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.MainMenu.Root
{
    public class MainMenuEnterParams : SceneEnterParams
    {
        public MapId TargetMapId { get; }

        public MainMenuEnterParams(MapId targetMapId) : base(targetMapId)
        {
            TargetMapId = targetMapId;
        }
    }
}