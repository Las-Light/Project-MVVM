using NothingBehind.Scripts.Game.GameRoot;

namespace NothingBehind.Scripts.Game.MainMenu.Root
{
    public class MainMenuEnterParams : SceneEnterParams
    {
        public string MapId { get; }

        public MainMenuEnterParams(string mapId) : base(mapId)
        {
            MapId = mapId;
        }
    }
}