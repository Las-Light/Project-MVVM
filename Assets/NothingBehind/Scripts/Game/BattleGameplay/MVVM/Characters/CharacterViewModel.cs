using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Characters
{
    public class CharacterViewModel
    {
        private readonly CharacterEntity _characterEntity;
        private readonly CharacterSettings _characterSettings;
        private readonly CharactersService _charactersService;
        private readonly InventoryViewModel _inventoryViewModel;
        private readonly Dictionary<int, CharacterLevelSettings> _levelSettingsMap = new();

        public readonly int CharacterEntityId;
        public readonly EntityType EntityType;
        public readonly ArsenalViewModel ArsenalViewModel;
        public ReadOnlyReactiveProperty<Vector3> Position { get; }
        public ReadOnlyReactiveProperty<int> Level { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }

        public CharacterViewModel(CharacterEntity characterEntity,
            CharacterSettings characterSettings,
            CharactersService charactersService, 
            InventoryViewModel inventoryViewModel,
            ArsenalViewModel arsenalViewModel)
        {
            EntityType = characterEntity.EntityType;
            CharacterEntityId = characterEntity.UniqueId;
            Level = characterEntity.Level;
            Health = characterEntity.Health;
            Position = characterEntity.Position;
            
            _characterEntity = characterEntity;
            _characterSettings = characterSettings;
            _charactersService = charactersService;
            _inventoryViewModel = inventoryViewModel;
            ArsenalViewModel = arsenalViewModel;

            foreach (var characterLevelSettings in characterSettings.Levels)
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