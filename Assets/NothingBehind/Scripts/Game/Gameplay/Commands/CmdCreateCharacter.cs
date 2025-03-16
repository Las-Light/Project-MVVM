using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdCreateCharacter : ICommand
    {
        public readonly EntityType CharacterType;
        public readonly int Level;
        public readonly Vector3 Position;
        public readonly InventoryService InventoryService;

        public CmdCreateCharacter(EntityType characterType, int level, Vector3 position,
            InventoryService inventoryService)
        {
            CharacterType = characterType;
            Level = level;
            Position = position;
            InventoryService = inventoryService;
        }

    }
}