using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.StoragesCommands
{
    public class CmdCreateStorage : ICommand
    {
        public readonly EntityType EntityType;
        public readonly Vector3 Position;
        public readonly InventoryService InventoryService;

        public CmdCreateStorage(EntityType entityType, Vector3 position, InventoryService inventoryService)
        {
            EntityType = entityType;
            Position = position;
            InventoryService = inventoryService;
        }
    }
}