using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Player
{
    public class CmdUpdatePlayerPosOnMap : ICommand
    {
        public readonly Vector3 Position;
        public readonly MapId CurrentMap;

        public CmdUpdatePlayerPosOnMap(Vector3 position, MapId currentMap)
        {
            Position = position;
            CurrentMap = currentMap;
        }
    }
}