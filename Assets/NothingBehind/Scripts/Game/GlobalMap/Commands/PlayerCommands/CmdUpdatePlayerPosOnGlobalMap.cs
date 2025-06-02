using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GlobalMap.Commands.PlayerCommands
{
    public class CmdUpdatePlayerPosOnGlobalMap : ICommand
    {
        public readonly Vector3 Position;
        public readonly MapId CurrentMap;

        public CmdUpdatePlayerPosOnGlobalMap(Vector3 position, MapId currentMap)
        {
            Position = position;
            CurrentMap = currentMap;
        }
    }
}