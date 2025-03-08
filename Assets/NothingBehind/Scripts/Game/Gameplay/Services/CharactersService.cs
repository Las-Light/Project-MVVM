using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class CharactersService
    {
        private readonly InventoryService _inventoryService;
        private readonly ICommandProcessor _commandProcessor;
        private readonly ObservableList<CharacterViewModel> _allCharacters = new();
        private readonly Dictionary<int, CharacterViewModel> _characterMap = new();
        private readonly Dictionary<string, CharacterSettings> _characterSettingsMap = new();

        public IObservableCollection<CharacterViewModel> AllCharacters => _allCharacters;

        public CharactersService(
            IObservableCollection<CharacterEntityProxy> characters,
            CharactersSettings charactersSettings,
            InventoryService inventoryService,
            ICommandProcessor commandProcessor)
        {
            _inventoryService = inventoryService;
            _commandProcessor = commandProcessor;

            foreach (var characterSettings in charactersSettings.AllCharacters)
            {
                _characterSettingsMap[characterSettings.TypeId] = characterSettings;
            }

            foreach (var characterEntity in characters)
            {
                CreateCharacterViewModel(characterEntity);
            }

            characters.ObserveAdd().Subscribe(e =>
            {
                CreateCharacterViewModel(e.Value);
            });

            characters.ObserveRemove().Subscribe(e =>
            {
                RemoveCharacterViewModel(e.Value);
            });
        }

        public bool CreateCharacter(string characterTypeId, int level, Vector3 position)
        {
            var command = new CmdCreateCharacter(characterTypeId, level, position, _inventoryService);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        public bool RemoveCharacter(int characterEntityId)
        {
            throw new NotImplementedException();
        }

        private void CreateCharacterViewModel(CharacterEntityProxy characterEntityProxy)
        {
            var characterSettings = _characterSettingsMap[characterEntityProxy.TypeId];
            var characterViewModel = new CharacterViewModel(characterEntityProxy, characterSettings, this);
            
            _allCharacters.Add(characterViewModel);
            _characterMap[characterEntityProxy.Id] = characterViewModel;
        }

        private void RemoveCharacterViewModel(CharacterEntityProxy characterEntityProxy)
        {
            if (_characterMap.TryGetValue(characterEntityProxy.Id, out var characterViewModel))
            {
                _allCharacters.Remove(characterViewModel);
                _characterMap.Remove(characterEntityProxy.Id);
            }
        }
    }
}