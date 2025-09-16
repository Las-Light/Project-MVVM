using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Weapons;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Weapons;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Services
{
    public class ArsenalService
    {
        public Dictionary<int, ArsenalViewModel> ArsenalMap => _arsenalMap;
        public IObservableCollection<ArsenalViewModel> AllArsenals => _allArsenals;

        private readonly EquipmentService _equipmentService;
        private readonly InventoryService _inventoryService;
        private readonly WeaponsSettings _weaponsSettings;
        private readonly ICommandProcessor _commandProcessor;
        private readonly ObservableList<ArsenalViewModel> _allArsenals = new();
        private readonly Dictionary<int, ArsenalViewModel> _arsenalMap = new();
        private readonly Dictionary<int, Arsenal> _arsenalDataMap = new();

        private readonly CompositeDisposable _disposables = new();

        public ArsenalService(ReadOnlyReactiveProperty<PlayerEntity> playerEntity, 
            IObservableCollection<Entity> entities,
            EquipmentService equipmentService,
            InventoryService inventoryService,
            WeaponsSettings weaponsSettings,
            ICommandProcessor commandProcessor)
        {
            _equipmentService = equipmentService;
            _inventoryService = inventoryService;
            _weaponsSettings = weaponsSettings;
            _commandProcessor = commandProcessor;
            
            // Initialize PlayerEntity
            var playerArsenal = playerEntity.CurrentValue.Arsenal.Value;
            _arsenalDataMap[playerArsenal.OwnerId] = playerArsenal;
            CreateArsenalViewModel(playerArsenal.OwnerId);

            // Initialize CharacterEntity

            foreach (var entity in entities)
            {
                if (entity is CharacterEntity characterEntity)
                {
                    var characterArsenal = characterEntity.Arsenal.Value;
                    _arsenalDataMap[characterArsenal.OwnerId] = characterArsenal;
                    CreateArsenalViewModel(characterArsenal.OwnerId);
                }
            }

            entities.ObserveAdd().Subscribe(e =>
            {
                var addedEntity = e.Value;
                if (addedEntity is CharacterEntity characterEntity)
                {
                    var arsenal = characterEntity.Arsenal.Value;
                    _arsenalDataMap[arsenal.OwnerId] = arsenal;
                    CreateArsenalViewModel(arsenal.OwnerId);
                }
            }).AddTo(_disposables);
            entities.ObserveRemove().Subscribe(e =>
            {
                var removedEntity = e.Value;
                if (removedEntity is CharacterEntity characterEntity)
                {
                    var equipment = characterEntity.Equipment.Value;
                    _arsenalDataMap.Remove(removedEntity.UniqueId);
                    RemoveArsenalViewModel(equipment.OwnerId);
                }
            }).AddTo(_disposables);
        }

        public ArsenalViewModel CreateArsenalViewModel(int ownerId)
        {
            if (_arsenalDataMap.TryGetValue(ownerId, out var arsenal))
            {
                if (_equipmentService.EquipmentMap.TryGetValue(ownerId, out var equipmentViewModel) &&
                    _inventoryService.InventoryMap.TryGetValue(ownerId, out var inventoryViewModel))
                {
                    var arsenalViewModel = new ArsenalViewModel(arsenal,
                        equipmentViewModel, inventoryViewModel, _commandProcessor, _weaponsSettings);

                    _allArsenals.Add(arsenalViewModel);
                    _arsenalMap[arsenal.OwnerId] = arsenalViewModel;
                    return arsenalViewModel;
                }
                Debug.LogError($"EquipmentViewModel {equipmentViewModel.OwnerId} or InventoryViewModel not found for ownerId {ownerId}!");
            }

            Debug.LogError($"ArsenalViewModel couldn't create, arsenal with ownerId {ownerId} not exist!");
            return null;
        }

        public void RemoveArsenalViewModel(int ownerId)
        {
            if (_arsenalMap.TryGetValue(ownerId, out var arsenalViewModel))
            {
                _allArsenals.Remove(arsenalViewModel);
                _arsenalMap.Remove(ownerId);
                arsenalViewModel.Dispose();
            }
        }
    }
}