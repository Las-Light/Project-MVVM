using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    public class CharacterViewModel
    {
        private readonly Character _character;
        private readonly CharacterSettings _characterSettings;
        private readonly CharactersService _charactersService;
        private readonly Dictionary<int, CharacterLevelSettings> _levelSettingsMap = new();

        public readonly int CharacterEntityId;
        public ReadOnlyReactiveProperty<Vector3> Position { get; }
        public ReadOnlyReactiveProperty<int> Level { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        public readonly string TypeId;

        public CharacterViewModel(
            Character character, 
            CharacterSettings characterSettings, 
            CharactersService charactersService)
        {
            TypeId = character.TypeId;
            CharacterEntityId = character.Id;
            Level = character.Level;
            Health = character.Health;
            
            _character = character;
            _characterSettings = characterSettings;
            _charactersService = charactersService;

            foreach (var characterLevelSettings in characterSettings.LevelSettings)
            {
                _levelSettingsMap[characterLevelSettings.Level] = characterLevelSettings;
            }

            Position = character.Position;
        }

        public CharacterLevelSettings GetLevelSettings(int level)
        {
            return _levelSettingsMap[level];
        }
    }
}