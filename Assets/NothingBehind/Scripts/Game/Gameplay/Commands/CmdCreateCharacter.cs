using NothingBehind.Scripts.Game.State.Commands;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdCreateCharacter : ICommand
    {
        public readonly string CharacterTypeId;
        public readonly int Level;
        public readonly float Health;
        public readonly Vector3Int Position;

        public CmdCreateCharacter(string characterTypeId, int level, float health, Vector3Int position)
        {
            CharacterTypeId = characterTypeId;
            Level = level;
            Health = health;
            Position = position;
        }

    }
}