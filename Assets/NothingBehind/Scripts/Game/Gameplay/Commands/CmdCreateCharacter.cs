using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Commands;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdCreateCharacter : ICommand
    {
        public readonly string CharacterTypeId;
        public readonly int Level;
        public readonly Vector3 Position;
        public readonly InventoryService InventoryService;

        public CmdCreateCharacter(string characterTypeId, int level, Vector3 position,
            InventoryService inventoryService)
        {
            CharacterTypeId = characterTypeId;
            Level = level;
            Position = position;
            InventoryService = inventoryService;
        }

    }
}