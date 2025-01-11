using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdCreateHero : ICommand
    {
        public MapId CurrentMapId;

        public CmdCreateHero(MapId currentMapId)
        {
            CurrentMapId = currentMapId;
        }
    }
}