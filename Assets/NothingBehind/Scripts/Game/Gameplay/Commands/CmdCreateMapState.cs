using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdCreateMapState : ICommand
    {
        public readonly int MapId;

        public CmdCreateMapState(int mapId)
        {
            MapId = mapId;
        }
    }
}