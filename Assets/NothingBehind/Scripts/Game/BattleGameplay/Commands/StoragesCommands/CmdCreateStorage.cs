using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.StoragesCommands
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