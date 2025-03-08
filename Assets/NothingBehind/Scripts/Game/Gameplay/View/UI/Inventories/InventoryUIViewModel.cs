using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.MVVM.UI;

namespace NothingBehind.Scripts.Game.Gameplay.View.UI.Inventories
{
    public class InventoryUIViewModel : WindowViewModel
    {
        public override string Id => "InventoryUI";
        
        private readonly InventoryService _inventoryService;
        public readonly int TargetOwnerId;
        public readonly int HeroId;

        public InventoryUIViewModel(InventoryService inventoryService, int targetOwnerId)
        {
            _inventoryService = inventoryService;
            HeroId = inventoryService.HeroId;
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


        public bool CreateInventory(string ownerTypeId, int ownerId)
        {
            return _inventoryService.CreateInventory(ownerTypeId, ownerId);
        }

        public bool RemoveInventory(int ownerId)
        {
            return _inventoryService.RemoveInventory(ownerId);
        }

        private InventoryViewModel CreateInventoryViewModel(int ownerId)
        {
            return _inventoryService.CreateInventoryViewModel(ownerId);
        }
    }
}