using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdCreateMapState : ICommand
    {
        public readonly string MapId;
        public readonly string SceneName;

        public CmdCreateMapState(string sceneName, string mapId)
        {
            SceneName = sceneName;
            MapId = mapId;
        }

    }
}