using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.EntityCommands;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Characters;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Services
{
    public class CharactersService
    {
        public IObservableCollection<CharacterViewModel> AllCharacters => _allCharacters;
        
        private readonly EquipmentService _equipmentService;
        private readonly InventoryService _inventoryService;
        private readonly ArsenalService _arsenalService;
        private readonly ICommandProcessor _commandProcessor;
        private readonly ObservableList<CharacterViewModel> _allCharacters = new();
        private readonly Dictionary<int, CharacterViewModel> _characterMap = new();
        private readonly Dictionary<EntityType, CharacterSettings> _characterSettingsMap = new();

        private readonly CompositeDisposable _disposables = new();
        
        public CharactersService(IObservableCollection<Entity> entities,
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

            foreach (var characterSettings in charactersSettings.Characters)
            {
                _characterSettingsMap[characterSettings.EntityType] = characterSettings;
            }

            foreach (var entity in entities)
            {
                if (entity is CharacterEntity characterEntity)
                {
                    CreateCharacterViewModel(characterEntity);
                }
            }

            entities.ObserveAdd().Subscribe(e =>
            {
                var addedEntity = e.Value;
                if (addedEntity is CharacterEntity characterEntity)
                {
                    CreateCharacterViewModel(characterEntity);
                }
            }).AddTo(_disposables);
            entities.ObserveRemove().Subscribe(e =>
            {
                var removedEntity = e.Value;
                if (removedEntity is CharacterEntity characterEntity)
                {
                    RemoveCharacterViewModel(characterEntity);
                }
            }).AddTo(_disposables);

            exitInventorRequest.Where(result => result.IsEmptyInventory && result.EntityType == EntityType.Character)
                .Subscribe(result =>
                {
                    RemoveEntity(result.OwnerId);
                });
        }

        public CommandResult CreateEntity(EntityType entityType, string configId, int level, Vector3 position)
        {
            var command = new CmdCreateEntity(entityType, configId, level, position);
            var result = _commandProcessor.Process(command);

            return result;
        }

        public CommandResult RemoveEntity(int entityId)
        {
            var command = new CmdRemoveEntity(entityId);
            var result = _commandProcessor.Process(command);

            return result;
        }

        private void CreateCharacterViewModel(CharacterEntity characterEntity)
        {
            var characterSettings = _characterSettingsMap[characterEntity.EntityType];
            if (!_inventoryService.InventoryMap.TryGetValue(characterEntity.UniqueId, out var inventoryViewModel))
            {
                Debug.LogError($"Inventory with Id - {characterEntity.UniqueId} not found");
            }
            if (!_arsenalService.ArsenalMap.TryGetValue(characterEntity.UniqueId, out var arsenalViewModel))
            {
                throw new Exception($"ArsenalViewModel for owner with Id {characterEntity.UniqueId} not found");
            }
            var characterViewModel = new CharacterViewModel(characterEntity,
                characterSettings, this, inventoryViewModel, arsenalViewModel);
            
            _allCharacters.Add(characterViewModel);
            _characterMap[characterEntity.UniqueId] = characterViewModel;
        }

        private void RemoveCharacterViewModel(CharacterEntity characterEntityEntityProxy)
        {
            if (_characterMap.TryGetValue(characterEntityEntityProxy.UniqueId, out var characterViewModel))
            {
                _allCharacters.Remove(characterViewModel);
                _characterMap.Remove(characterEntityEntityProxy.UniqueId);
            }
        }
    }
}