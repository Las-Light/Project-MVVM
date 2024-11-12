using NothingBehind.Scripts.Game.State.Commands;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdCreateCharacter : ICommand
    {
        public readonly string CharacterTypeId;
        public readonly Vector3Int Position;

        public CmdCreateCharacter(string characterTypeId, Vector3Int position)
        {
            CharacterTypeId = characterTypeId;
            Position = position;
        }
    }
}