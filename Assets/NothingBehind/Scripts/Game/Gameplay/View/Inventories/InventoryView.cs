using NothingBehind.Scripts.MVVM.UI;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class InventoryView : PopupBinder<InventoryViewModel>
    {
        [SerializeField] private GameObject _gridPrefab;
        [SerializeField] private RectTransform _gridContainer;
        
        private InventoryViewModel _inventoryViewModel;

        protected override void OnBind(InventoryViewModel viewModel)
        {
            base.OnBind(viewModel);
            _inventoryViewModel = viewModel;
            foreach (var inventoryGridViewModel in ViewModel.AllInventoryGrids)
            {
                CreateInventorGridView(inventoryGridViewModel);
            }
        }

        public InventoryViewModel GetInventoryViewModel()
        {
            return _inventoryViewModel;
        }

        private void CreateInventorGridView(InventoryGridViewModel inventoryGridViewModel)
        {
            var itemView = Instantiate(_gridPrefab, _gridContainer);
            itemView.GetComponent<InventoryGridView>().Bind(inventoryGridViewModel);
        }
    }
}