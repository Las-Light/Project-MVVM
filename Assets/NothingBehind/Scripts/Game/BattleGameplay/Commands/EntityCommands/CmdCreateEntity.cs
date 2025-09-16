using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.EntityCommands
{
    public class CmdCreateEntity : ICommand
    {
        public readonly EntityType EntityType;
        public readonly string ConfigId;
        public readonly int Level;
        public readonly Vector3 Position;

        public CmdCreateEntity(EntityType entityType, string configId, int level, Vector3 position)
        {
            EntityType = entityType;
            ConfigId = configId;
            Level = level;
            Position = position;
        }
    }
}