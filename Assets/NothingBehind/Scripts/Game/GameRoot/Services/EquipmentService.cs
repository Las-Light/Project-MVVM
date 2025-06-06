using System.Collections.Generic;
using NothingBehind.Scripts.Game.GameRoot.Commands.EquipmentCommands;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Equipments;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.Services
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

        public EquipmentService(IObservableCollection<Equipment> equipments,
            ItemsSettings itemsSettings,
            ICommandProcessor commandProcessor)
        {
            _itemsSettings = itemsSettings;
            _commandProcessor = commandProcessor;
            
            foreach (var equipment in equipments)
            {
                _equipmentsDataMap[equipment.OwnerId] = equipment;
                CreateEquipmentViewModel(equipment.OwnerId);
            }

            equipments.ObserveAdd().Subscribe(e =>
            {
                _equipmentsDataMap[e.Value.OwnerId] = e.Value;
                CreateEquipmentViewModel(e.Value.OwnerId);
            });
            equipments.ObserveRemove().Subscribe(e =>
            {
                _equipmentsDataMap.Remove(e.Value.OwnerId);
                RemoveEquipmentViewModel(e.Value);
            });
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

        public CommandResult CreateEquipment(int ownerId, EntityType entityType)
        {
            var command = new CmdCreateEquipment(ownerId, entityType);
            var result = _commandProcessor.Process(command);
            Debug.Log("Create equip");
            
            return result;
        }

        public CommandResult RemoveEquipment(int ownerId)
        {
            var command = new CmdRemoveEquipment(ownerId);
            var result = _commandProcessor.Process(command);
            
            return result;
        }
        
        public EquipmentViewModel CreateEquipmentViewModel(int ownerId)
        {
            if (_equipmentsDataMap.TryGetValue(ownerId, out var equipment))
            {
                var inventoryViewModel = new EquipmentViewModel(equipment, 
                    _itemsSettings,
                    this,
                    _commandProcessor);

                _allEquipmentViewModels.Add(inventoryViewModel);
                _equipmentMap[equipment.OwnerId] = inventoryViewModel;
                return inventoryViewModel;
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
    }
}