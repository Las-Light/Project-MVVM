using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands.EquipmentCommands;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class EquipmentService
    {
        public int PlayerId { get; }
        
        private readonly ICommandProcessor _commandProcessor;
        private readonly ObservableList<EquipmentViewModel> _allEquipmentViewModels = new();
        private readonly Dictionary<int, EquipmentViewModel> _equipmentViewModelsMap = new();
        private readonly Dictionary<int, Equipment> _equipmentsDataMap = new();


        public IObservableCollection<EquipmentViewModel> AllEquipmentViewModels => _allEquipmentViewModels;

        public Dictionary<int, EquipmentViewModel> EquipmentViewModelsMap => _equipmentViewModelsMap;

        public EquipmentService(IObservableCollection<Equipment> equipments,
            ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
            
            foreach (var equipmentDataProxy in equipments)
            {
                _equipmentsDataMap[equipmentDataProxy.OwnerId] = equipmentDataProxy;
                CreateEquipmentViewModel(equipmentDataProxy.OwnerId);
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
        
        public CommandResult CreateEquipment(int ownerId, EntityType entityType)
        {
            var command = new CmdCreateEquipment(ownerId, entityType);
            var result = _commandProcessor.Process(command);
            
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
            if (_equipmentsDataMap.TryGetValue(ownerId, out var equipmentDataProxy))
            {
                var inventoryViewModel = new EquipmentViewModel(equipmentDataProxy,
                    this);

                _allEquipmentViewModels.Add(inventoryViewModel);
                _equipmentViewModelsMap[equipmentDataProxy.OwnerId] = inventoryViewModel;
                return inventoryViewModel;
            }

            return null;
        }

        public void RemoveEquipmentViewModel(Equipment equipment)
        {
            if (_equipmentViewModelsMap.TryGetValue(equipment.OwnerId, out var equipmentViewModel))
            {
                _allEquipmentViewModels.Remove(equipmentViewModel);
                _equipmentViewModelsMap.Remove(equipment.OwnerId);
                equipmentViewModel.Dispose();
            }
        }
    }
}