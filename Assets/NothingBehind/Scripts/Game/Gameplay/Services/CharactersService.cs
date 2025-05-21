using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands.CharactersCommands;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class CharactersService
    {
        private readonly EquipmentService _equipmentService;
        private readonly InventoryService _inventoryService;
        private readonly ArsenalService _arsenalService;
        private readonly ICommandProcessor _commandProcessor;
        private readonly ObservableList<CharacterViewModel> _allCharacters = new();
        private readonly Dictionary<int, CharacterViewModel> _characterMap = new();
        private readonly Dictionary<EntityType, CharacterSettings> _characterSettingsMap = new();

        public IObservableCollection<CharacterViewModel> AllCharacters => _allCharacters;

        public CharactersService(IObservableCollection<Character> characters,
            CharactersSettings charactersSettings,
            EquipmentService equipmentService,
            InventoryService inventoryService,
            ArsenalService arsenalService,
            ICommandProcessor commandProcessor,
            Subject<ExitInventoryRequestResult> exitInventorRequest)
        {
            _equipmentService = equipmentService;
            _inventoryService = inventoryService;
            _arsenalService = arsenalService;
            _commandProcessor = commandProcessor;

            foreach (var characterSettings in charactersSettings.AllCharacters)
            {
                _characterSettingsMap[characterSettings.EntityType] = characterSettings;
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
            
            exitInventorRequest.Where(result => result.IsEmptyInventory && result.EntityType == EntityType.Character)
                .Subscribe(result =>
                {
                    RemoveCharacter(result.OwnerId);
                });
        }

        public CommandResult CreateCharacter(EntityType characterType, int level, Vector3 position)
        {
            var command = new CmdCreateCharacter(characterType, level,
                position, _equipmentService, _inventoryService, _arsenalService);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        public CommandResult RemoveCharacter(int characterEntityId)
        {
            var command = new CmdRemoveCharacter(characterEntityId, _inventoryService, _equipmentService, _arsenalService);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        private void CreateCharacterViewModel(Character character)
        {
            var characterSettings = _characterSettingsMap[character.EntityType];
            if (!_inventoryService.InventoryMap.TryGetValue(character.Id, out var inventoryViewModel))
            {
                Debug.LogError($"Inventory with Id - {character.Id} not found");
            }
            if (!_arsenalService.ArsenalMap.TryGetValue(character.Id, out var arsenalViewModel))
            {
                throw new Exception($"ArsenalViewModel for owner with Id {character.Id} not found");
            }
            var characterViewModel = new CharacterViewModel(character,
                characterSettings, this, inventoryViewModel, arsenalViewModel);
            
            _allCharacters.Add(characterViewModel);
            _characterMap[character.Id] = characterViewModel;
        }

        private void RemoveCharacterViewModel(Character characterEntityProxy)
        {
            if (_characterMap.TryGetValue(characterEntityProxy.Id, out var characterViewModel))
            {
                _allCharacters.Remove(characterViewModel);
                _characterMap.Remove(characterEntityProxy.Id);
            }
        }
    }
}