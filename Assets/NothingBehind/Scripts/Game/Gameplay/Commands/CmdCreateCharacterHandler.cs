using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
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
            var entityId = _gameState.GetEntityID();
            var newCharacterEntity = new CharacterEntity
            {
                Id = entityId,
                Position = command.Position,
                TypeID = command.CharacterTypeId
            };

            var newCharacterEntityProxy = new CharacterEntityProxy(newCharacterEntity);
            _gameState.Characters.Add(newCharacterEntityProxy);

            return true; // тут может быть валидация на создание сущности
        }
    }
}