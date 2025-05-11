using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.MVVM.UI;
using NothingBehind.Scripts.Utils;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.UI.Inventories
{
    public class InventoryUIViewModel : WindowViewModel
    {
        public override string Id => "InventoryUI";
        public readonly int OwnerId;
        public GameplayInputManager GameplayInputManager => _gameplayInputManager;

        private readonly InventoryService _inventoryService;
        private readonly EquipmentService _equipmentService;
        private readonly StorageService _storageService;
        private readonly GameplayInputManager _gameplayInputManager;
        public readonly int TargetOwnerId;
        private readonly Subject<ExitInventoryRequestResult> _exitInventoryRequest;
        private readonly Vector3 _ownerPosition;

        public InventoryUIViewModel(InventoryService inventoryService,
            EquipmentService equipmentService,
            StorageService storageService,
            int ownerId,
            Vector3 ownerPosition,
            int targetOwnerId,
            Subject<ExitInventoryRequestResult> exitInventoryRequest, 
            GameplayInputManager gameplayInputManager)
        {
            _inventoryService = inventoryService;
            _equipmentService = equipmentService;
            _storageService = storageService;
            _ownerPosition = ownerPosition;
            OwnerId = ownerId;
            TargetOwnerId = targetOwnerId;
            _exitInventoryRequest = exitInventoryRequest;
            _gameplayInputManager = gameplayInputManager;
            _gameplayInputManager.PlayerInputDisabled();
        }


        public InventoryViewModel GetInventoryViewModel(int ownerId)
        {
            if (_inventoryService.InventoryMap.TryGetValue(ownerId, out var viewModel))
            {
                return viewModel;
            }
            return CreateInventoryViewModel(ownerId);
        }
        
        public EquipmentViewModel GetEquipmentViewModel(int ownerId)
        {
            if (_equipmentService.EquipmentMap.TryGetValue(ownerId, out var viewModel))
            {
                return viewModel;
            }

            return CreateEquipmentViewModel(ownerId);
        }

        public CommandResult CreateStorage()
        {
            return _storageService.CreateStorage(EntityType.Storage, _ownerPosition);
        }

        public void GetExitInventoryRequest(int ownerId)
        {
            var inventoryViewModel = GetInventoryViewModel(ownerId);
            var isEmptyInventory = inventoryViewModel.IsEmptyInventory();
            _exitInventoryRequest.OnNext(new ExitInventoryRequestResult(isEmptyInventory, ownerId, inventoryViewModel.OwnerType));
        }

        private EquipmentViewModel CreateEquipmentViewModel(int ownerId)
        {
            return _equipmentService.CreateEquipmentViewModel(ownerId);
        }

        private InventoryViewModel CreateInventoryViewModel(int ownerId)
        {
            return _inventoryService.CreateInventoryViewModel(ownerId);
        }

        public override void Dispose()
        {
            base.Dispose();
            _gameplayInputManager.PlayerInputEnabled();
            _gameplayInputManager.UIInputDisabled();
        }
    }
}