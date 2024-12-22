using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdCreateMapState : ICommand
    {
        public readonly MapId MapId;

        public CmdCreateMapState(MapId mapId)
        {
            MapId = mapId;
        }

    }
}