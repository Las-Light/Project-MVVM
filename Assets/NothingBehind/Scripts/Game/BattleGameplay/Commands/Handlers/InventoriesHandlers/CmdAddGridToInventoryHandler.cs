using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.InventoriesHandlers
{
    public class CmdAddGridToInventoryHandler : ICommandHandler<CmdAddGridToInventory>
    {
        private readonly GameStateProxy _gameState;

        public CmdAddGridToInventoryHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public CommandResult Handle(CmdAddGridToInventory command)
        {
            Inventory inventory;
            switch (command.EntityType)
            {
                case EntityType.Player:
                    inventory = _gameState.Player.CurrentValue.Inventory.CurrentValue;
                    break;
                case EntityType.Character:
                {
                    var currentMap = _gameState.Maps.FirstOrDefault(m => m.Id == _gameState.CurrentMapId.CurrentValue);
                    if (currentMap == null)
                    {
                        Debug.Log($"Couldn't find MapState for ID: {_gameState.CurrentMapId.CurrentValue}");
                        return new CommandResult(false);
                    }

                    var entity = currentMap.Entities.FirstOrDefault(c => c.UniqueId == command.OwnerId);
                    if (entity is CharacterEntity characterEntity)
                    {
                        inventory = characterEntity.Inventory.CurrentValue;
                        break;                        
                    }
                    Debug.Log($"Couldn't find Character for ID: {command.OwnerId}");
                    return new CommandResult(false);
                }
                case EntityType.Storage:
                {
                    var currentMap = _gameState.Maps.FirstOrDefault(m => m.Id == _gameState.CurrentMapId.CurrentValue);
                    if (currentMap == null)
                    {
                        Debug.Log($"Couldn't find MapState for ID: {_gameState.CurrentMapId.CurrentValue}");
                        return new CommandResult(false);
                    }

                    var entity = currentMap.Entities.FirstOrDefault(s => s.UniqueId == command.OwnerId);
                    if (entity is StorageEntity storageEntity)
                    {
                        inventory = storageEntity.Inventory.CurrentValue;
                        break;
                    }
                    Debug.Log($"Couldn't find Character for ID: {command.OwnerId}");
                    return new CommandResult(false);
                }
                default:
                {
                    Debug.Log($"Couldn't find EntityType for ID: {command.OwnerId} with Type: {command.EntityType}");
                    return new CommandResult(false);
                }
            }

            if (inventory.InventoryGrids.FirstOrDefault(grid => grid.GridId == command.Grid.GridId) != null)
            {
                Debug.LogError(
                    $"InventoryGrid with Id {command.Grid} already exists in Inventory {inventory.OwnerType}-{inventory.OwnerId}.");
                return new CommandResult(command.Grid.GridId,false);
            }

            if (command.Grid is InventoryGridWithSubGrid subGrid)
            {
                inventory.InventoryGrids.Add(subGrid);
            }
            else
            {
                inventory.InventoryGrids.Add(command.Grid);
            }

            return new CommandResult(command.Grid.GridId, true);
        }
    }
}