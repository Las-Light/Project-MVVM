using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Characters
{
    public class CharacterViewModel
    {
        private readonly Character _character;
        private readonly CharacterSettings _characterSettings;
        private readonly CharactersService _charactersService;
        private readonly InventoryViewModel _inventoryViewModel;
        private readonly Dictionary<int, CharacterLevelSettings> _levelSettingsMap = new();

        public readonly int CharacterEntityId;
        public readonly EntityType Type;
        public ReadOnlyReactiveProperty<Vector3> Position { get; }
        public ReadOnlyReactiveProperty<int> Level { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }

        public CharacterViewModel(Character character,
            CharacterSettings characterSettings,
            CharactersService charactersService, 
            InventoryViewModel inventoryViewModel)
        {
            Type = character.EntityType;
            CharacterEntityId = character.Id;
            Level = character.Level;
            Health = character.Health;
            Position = character.Position;
            
            _character = character;
            _characterSettings = characterSettings;
            _charactersService = charactersService;
            _inventoryViewModel = inventoryViewModel;

            foreach (var characterLevelSettings in characterSettings.LevelSettings)
            {
                _levelSettingsMap[characterLevelSettings.Level] = characterLevelSettings;
            }
        }
        
        public bool IsEmptyInventory()
        {
            return _inventoryViewModel.IsEmptyInventory();
        }

        public CharacterLevelSettings GetLevelSettings(int level)
        {
            return _levelSettingsMap[level];
        }
    }
}