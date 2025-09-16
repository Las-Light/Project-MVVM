using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Equipments;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Services
{
    public class EquipmentService
    {
        public int PlayerId { get; }
        public IObservableCollection<EquipmentViewModel> AllEquipmentViewModels => _allEquipmentViewModels;
        public Dictionary<int, EquipmentViewModel> EquipmentMap => _equipmentMap;

        private readonly ItemsSettings _itemsSettings;
        private readonly ICommandProcessor _commandProcessor;
        private readonly ObservableList<EquipmentViewModel> _allEquipmentViewModels = new();
        private readonly Dictionary<int, EquipmentViewModel> _equipmentMap = new();
        private readonly Dictionary<int, Equipment> _equipmentsDataMap = new();

        private CompositeDisposable _disposables = new();

        public EquipmentService(ReadOnlyReactiveProperty<PlayerEntity> playerEntity,
            IObservableCollection<Entity> entities,
            ItemsSettings itemsSettings,
            ICommandProcessor commandProcessor)
        {
            _itemsSettings = itemsSettings;
            _commandProcessor = commandProcessor;

            // Initialize PlayerEntity
            PlayerId = playerEntity.CurrentValue.UniqueId;
            var playerEquipment = playerEntity.CurrentValue.Equipment.Value;
            _equipmentsDataMap[PlayerId] = playerEquipment;
            CreateEquipmentViewModel(PlayerId);

            // Initialize CharacterEntity
            foreach (var entity in entities)
            {
                if (entity is CharacterEntity characterEntity)
                {
                    var equipment = characterEntity.Equipment.Value;
                    _equipmentsDataMap[equipment.OwnerId] = equipment;
                    CreateEquipmentViewModel(equipment.OwnerId);
                }
            }

            entities.ObserveAdd().Subscribe(e =>
            {
                var addedEntity = e.Value;
                if (addedEntity is CharacterEntity characterEntity)
                {
                    var equipment = characterEntity.Equipment.Value;
                    _equipmentsDataMap[equipment.OwnerId] = equipment;
                    CreateEquipmentViewModel(equipment.OwnerId);
                }
            }).AddTo(_disposables);
            entities.ObserveRemove().Subscribe(e =>
            {
                var removedEntity = e.Value;
                if (removedEntity is CharacterEntity characterEntity)
                {
                    var equipment = characterEntity.Equipment.Value;
                    _equipmentsDataMap.Remove(removedEntity.UniqueId);
                    RemoveEquipmentViewModel(equipment);
                }
            }).AddTo(_disposables);
        }

        public void UpdateEquipmentService(IObservableCollection<Equipment> newEquipments)
        {
            // Очищаем данные и отписываемся от старой коллекции
            ClearCurrentData();
            _disposables.Dispose();
            _disposables = new CompositeDisposable();

            // Обновляем ссылку и подписываемся на новую коллекцию
            foreach (var equipment in newEquipments)
            {
                _equipmentsDataMap[equipment.OwnerId] = equipment;
                CreateEquipmentViewModel(equipment.OwnerId);
            }

            newEquipments.ObserveAdd().Subscribe(e =>
            {
                var addedEquipment = e.Value;
                _equipmentsDataMap[addedEquipment.OwnerId] = addedEquipment;
                CreateEquipmentViewModel(addedEquipment.OwnerId);
            }).AddTo(_disposables);

            newEquipments.ObserveRemove().Subscribe(e =>
            {
                var removedEquipment = e.Value;
                _equipmentsDataMap.Remove(removedEquipment.OwnerId);
                RemoveEquipmentViewModel(removedEquipment);
            }).AddTo(_disposables);
        }

        public EquipmentViewModel CreateEquipmentViewModel(int ownerId)
        {
            if (_equipmentsDataMap.TryGetValue(ownerId, out var equipment))
            {
                var equipmentViewModel = new EquipmentViewModel(equipment,
                    _itemsSettings,
                    this,
                    _commandProcessor);

                _allEquipmentViewModels.Add(equipmentViewModel);
                _equipmentMap[equipment.OwnerId] = equipmentViewModel;
                return equipmentViewModel;
            }

            Debug.LogError($"EquipmentViewModel couldn't create, equipment with ownerId {ownerId} not exist!");
            return null;
        }

        public void RemoveEquipmentViewModel(Equipment equipment)
        {
            if (_equipmentMap.TryGetValue(equipment.OwnerId, out var equipmentViewModel))
            {
                _allEquipmentViewModels.Remove(equipmentViewModel);
                _equipmentMap.Remove(equipment.OwnerId);
                //equipmentViewModel.Dispose();
            }
        }

        private void ClearCurrentData()
        {
            foreach (var equipment in _equipmentsDataMap.Values)
            {
                RemoveEquipmentViewModel(equipment);
            }

            _equipmentsDataMap.Clear();
            _allEquipmentViewModels.Clear();
            _equipmentMap.Clear();
        }
    }
}