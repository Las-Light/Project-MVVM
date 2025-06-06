using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.StoragesCommands;
using NothingBehind.Scripts.Game.Settings.Gameplay.Storages;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.StoragesHandlers
{
    public class CmdCreateStorageHandler : ICommandHandler<CmdCreateStorage>
    {
        private readonly GameStateProxy _gameState;
        private readonly StoragesSettings _storagesSettings;

        public CmdCreateStorageHandler(GameStateProxy gameState,
            StoragesSettings storagesSettings)
        {
            _gameState = gameState;
            _storagesSettings = storagesSettings;
        }
        public CommandResult Handle(CmdCreateStorage command)
        {
            var currentMap = _gameState.Maps.FirstOrDefault(m => m.Id == _gameState.CurrentMapId.CurrentValue);
            if (currentMap == null)
            {
                Debug.Log($"Couldn't find Mapstate for ID: {_gameState.CurrentMapId.CurrentValue}");
                return new CommandResult( false);
            }
            var entityId = _gameState.CreateEntityId();
            var storageSettings = _storagesSettings.Storages.First(c=>c.EntityType == command.EntityType);
            var storageData = new StorageData
            {
                UniqueId = entityId,
                Position = command.Position,
                EntityType = command.EntityType
            };

            var newStorage = new Storage(storageData);
            command.InventoryService.CreateInventory(command.EntityType, entityId);
            currentMap.Storages.Add(newStorage);

            return new CommandResult(entityId, true); // тут может быть валидация на создание сущности
        }
    }
}