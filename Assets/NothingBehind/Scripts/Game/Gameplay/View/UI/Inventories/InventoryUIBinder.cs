using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.MVVM.UI;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.UI.Inventories
{
    public class InventoryUIBinder : PopupBinder<InventoryUIViewModel>
    {
        [SerializeField] private GameObject _inventoryPrefab;
        [SerializeField] private RectTransform _playerInventoryContainer;
        [SerializeField] private RectTransform _lootInventoryContainer;

        protected override void OnBind(InventoryUIViewModel viewModel)
        {
            base.OnBind(viewModel);
            if (viewModel.PlayerId == viewModel.TargetOwnerId)
            {
                CreatePlayerInventoryView(viewModel.GetInventoryViewModel(viewModel.PlayerId));
            }
            else
            {
                CreatePlayerInventoryView(viewModel.GetInventoryViewModel(viewModel.PlayerId));
                CreateLootInventoryView(viewModel.GetInventoryViewModel(viewModel.TargetOwnerId));
            }
        }

        private void CreatePlayerInventoryView(InventoryViewModel inventoryView)
        {
            var itemView = Instantiate(_inventoryPrefab, _playerInventoryContainer);
            itemView.GetComponent<InventoryView>().Bind(inventoryView);
        }

        private void CreateLootInventoryView(InventoryViewModel inventoryView)
        {
            var itemView = Instantiate(_inventoryPrefab, _lootInventoryContainer);
            itemView.GetComponent<InventoryView>().Bind(inventoryView);
        }
    }
}