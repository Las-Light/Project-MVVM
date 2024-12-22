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
        private readonly CharacterEntityProxy _characterEntityProxy;
        private readonly CharacterSettings _characterSettings;
        private readonly CharactersService _charactersService;
        private readonly Dictionary<int, CharacterLevelSettings> _levelSettingsMap = new();

        public readonly int CharacterEntityId;
        public ReadOnlyReactiveProperty<Vector3Int> Position { get; }
        public ReadOnlyReactiveProperty<int> Level { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        public readonly string TypeId;

        public CharacterViewModel(
            CharacterEntityProxy characterEntityProxy, 
            CharacterSettings characterSettings, 
            CharactersService charactersService)
        {
            TypeId = characterEntityProxy.TypeId;
            CharacterEntityId = characterEntityProxy.Id;
            Level = characterEntityProxy.Level;
            Health = characterEntityProxy.Health;
            
            _characterEntityProxy = characterEntityProxy;
            _characterSettings = characterSettings;
            _charactersService = charactersService;

            foreach (var characterLevelSettings in characterSettings.LevelSettings)
            {
                _levelSettingsMap[characterLevelSettings.Level] = characterLevelSettings;
            }

            Position = characterEntityProxy.Position;
        }

        public CharacterLevelSettings GetLevelSettings(int level)
        {
            return _levelSettingsMap[level];
        }
    }
}