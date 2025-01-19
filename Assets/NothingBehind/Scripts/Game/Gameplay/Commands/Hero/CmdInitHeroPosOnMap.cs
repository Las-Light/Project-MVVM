using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Hero
{
    public class CmdInitHeroPosOnMap : ICommand
    {
        public MapId CurrentMapId;

        public CmdInitHeroPosOnMap(MapId currentMapId)
        {
            CurrentMapId = currentMapId;
        }
    }
}