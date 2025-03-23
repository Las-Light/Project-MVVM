using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.View.Equipments;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.MVVM.UI;

namespace NothingBehind.Scripts.Game.Gameplay.View.UI.Inventories
{
    public class InventoryUIViewModel : WindowViewModel
    {
        public override string Id => "InventoryUI";
        
        private readonly InventoryService _inventoryService;
        private readonly EquipmentService _equipmentService;
        public readonly int TargetOwnerId;
        public readonly int PlayerId;

        public InventoryUIViewModel(
            InventoryService inventoryService,
            EquipmentService equipmentService, 
            int targetOwnerId)
        {
            _inventoryService = inventoryService;
            _equipmentService = equipmentService;
            PlayerId = inventoryService.PlayerId;
            TargetOwnerId = targetOwnerId;
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
            if (_equipmentService.EquipmentViewModelsMap.TryGetValue(ownerId, out var viewModel))
            {
                return viewModel;
            }

            return CreateEquipmentViewModel(ownerId);
        }

        public bool CreateInventory(EntityType ownerTypeId, int ownerId)
        {
            return _inventoryService.CreateInventory(ownerTypeId, ownerId);
        }

        public bool RemoveInventory(int ownerId)
        {
            return _inventoryService.RemoveInventory(ownerId);
        }

        private EquipmentViewModel CreateEquipmentViewModel(int ownerId)
        {
            return _equipmentService.CreateEquipmentViewModel(ownerId);
        }

        private InventoryViewModel CreateInventoryViewModel(int ownerId)
        {
            return _inventoryService.CreateInventoryViewModel(ownerId);
        }
    }
}