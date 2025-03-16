using NothingBehind.Scripts.Game.State.Commands;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Hero
{
    public class CmdUpdatePlayerPosOnMap : ICommand
    {
        public readonly Vector3 Position;

        public CmdUpdatePlayerPosOnMap(Vector3 position)
        {
            Position = position;
        }
    }
}