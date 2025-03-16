using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Hero
{
    public class CmdInitPlayerPosOnMap : ICommand
    {
        public MapId CurrentMapId;

        public CmdInitPlayerPosOnMap(MapId currentMapId)
        {
            CurrentMapId = currentMapId;
        }
    }
}