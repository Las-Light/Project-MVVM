using System.Linq;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers
{
    public class CmdCreateCharacterHandler : ICommandHandler<CmdCreateCharacter>
    {
        private readonly GameStateProxy _gameState;

        public CmdCreateCharacterHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        
        public bool Handle(CmdCreateCharacter command)
        {
            var currentMap = _gameState.Maps.FirstOrDefault(m => m.Id == _gameState.CurrentMapId.CurrentValue);
            if (currentMap == null)
            {
                Debug.Log($"Couldn't find Mapstate for ID: {_gameState.CurrentMapId.CurrentValue}");
                return false;
            }
            var entityId = _gameState.CreateEntityId();
            var newCharacterEntity = new CharacterEntity
            {
                Id = entityId,
                Position = command.Position,
                Level = command.Level,
                Health = command.Health,
                TypeId = command.CharacterTypeId
            };

            var newCharacterEntityProxy = new CharacterEntityProxy(newCharacterEntity);
            
            currentMap.Characters.Add(newCharacterEntityProxy);

            return true; // тут может быть валидация на создание сущности
        }
    }
}