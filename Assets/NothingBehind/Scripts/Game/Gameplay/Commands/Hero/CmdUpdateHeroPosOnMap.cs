using NothingBehind.Scripts.Game.State.Commands;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Hero
{
    public class CmdUpdateHeroPosOnMap : ICommand
    {
        public readonly Vector3 Position;

        public CmdUpdateHeroPosOnMap(Vector3 position)
        {
            Position = position;
        }
    }
}