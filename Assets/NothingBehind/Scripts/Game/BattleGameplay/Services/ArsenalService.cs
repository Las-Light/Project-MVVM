using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Weapons;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Utils;
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

        public ArsenalService(IObservableCollection<Arsenal> arsenals,
            EquipmentService equipmentService,
            InventoryService inventoryService,
            WeaponsSettings weaponsSettings,
            ICommandProcessor commandProcessor)
        {
            _equipmentService = equipmentService;
            _inventoryService = inventoryService;
            _weaponsSettings = weaponsSettings;
            _commandProcessor = commandProcessor;
            foreach (var arsenal in arsenals)
            {
                _arsenalDataMap[arsenal.OwnerId] = arsenal;
                CreateArsenalViewModel(arsenal.OwnerId);
            }

            arsenals.ObserveAdd().Subscribe(e =>
            {
                var addedArsenal = e.Value;
                _arsenalDataMap[addedArsenal.OwnerId] = addedArsenal;
                CreateArsenalViewModel(addedArsenal.OwnerId);
            });
            arsenals.ObserveRemove().Subscribe(e =>
            {
                var removedArsenal = e.Value;
                _arsenalDataMap.Remove(removedArsenal.OwnerId);
                RemoveArsenalViewModel(removedArsenal.OwnerId);
            });
        }

        public CommandResult CreateArsenal(int ownerId)
        {
            var command = new CmdCreateArsenal(ownerId);
            var result = _commandProcessor.Process(command);

            return result;
        }

        public CommandResult RemoveArsenal(int ownerId)
        {
            var command = new CmdRemoveArsenal(ownerId);
            var result = _commandProcessor.Process(command);

            return result;
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