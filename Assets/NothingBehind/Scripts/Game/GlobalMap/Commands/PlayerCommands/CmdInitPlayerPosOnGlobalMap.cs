using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.GlobalMap.Commands.PlayerCommands
{
    public class CmdInitPlayerPosOnGlobalMap: ICommand
    {
        public readonly MapId CurrentMapId;

        public CmdInitPlayerPosOnGlobalMap(MapId currentMapId)
        {
            CurrentMapId = currentMapId;
        }
    }
}