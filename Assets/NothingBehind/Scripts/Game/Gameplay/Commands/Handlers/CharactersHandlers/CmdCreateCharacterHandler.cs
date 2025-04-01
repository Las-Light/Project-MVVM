using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.CharactersCommands;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.CharactersHandlers
{
    public class CmdCreateCharacterHandler : ICommandHandler<CmdCreateCharacter>
    {
        private readonly GameStateProxy _gameState;
        private readonly CharactersSettings _charactersSettings;

        public CmdCreateCharacterHandler(GameStateProxy gameState,
            CharactersSettings charactersSettings)
        {
            _gameState = gameState;
            _charactersSettings = charactersSettings;
        }
        
        public CommandResult Handle(CmdCreateCharacter command)
        {
            var currentMap = _gameState.Maps.FirstOrDefault(m => m.Id == _gameState.CurrentMapId.CurrentValue);
            if (currentMap == null)
            {
                Debug.Log($"Couldn't find Mapstate for ID: {_gameState.CurrentMapId.CurrentValue}");
                return new CommandResult(false);
            }
            var entityId = _gameState.CreateEntityId();
            var characterSettings = _charactersSettings.AllCharacters.First(c=>c.EntityType == command.CharacterType);
            var characterLevel = characterSettings.LevelSettings.First(l => l.Level == command.Level);
            var characterData = new CharacterData
            {
                UniqueId = entityId,
                Position = command.Position,
                Level = command.Level,
                Health = characterLevel.Health,
                EntityType = command.CharacterType
            };

            var newCharacter = new Character(characterData);
            command.EquipmentService.CreateEquipment(entityId, command.CharacterType);
            command.InventoryService.CreateInventory(command.CharacterType, entityId);
            currentMap.Characters.Add(newCharacter);

            return new CommandResult(entityId, true); // тут может быть валидация на создание сущности
        }
    }
}