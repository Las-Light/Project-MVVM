using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdCreateMapState : ICommand
    {
        public readonly string MapId;

        public CmdCreateMapState(string mapId)
        {
            MapId = mapId;
        }
    }
}