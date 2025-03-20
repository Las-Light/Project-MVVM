using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private GameObject _gridPrefab;
        [SerializeField] private GameObject _subGridPrefab;
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private RectTransform _subGridContainer;
        public int OwnerId { get; set; }

        private InventoryViewModel _inventoryViewModel;


        public void Bind(InventoryViewModel viewModel)
        {
            _inventoryViewModel = viewModel;
            OwnerId = viewModel.OwnerId;
            foreach (var inventoryGridViewModel in viewModel.AllInventoryGrids)
            {
                if (inventoryGridViewModel.IsSubGrid)
                {
                    CreateInventorSubGridView(inventoryGridViewModel);
                }
                else
                {
                    CreateInventorGridView(inventoryGridViewModel);
                }
            }

            // Задаем размер инвентаря в соответствии с размером экрана
            var viewScreenSize = new Vector2(Screen.width / 3, Screen.height);
            transform.parent.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            _gridContainer.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
        }

        private void CreateInventorGridView(InventoryGridViewModel inventoryGridViewModel)
        {
            var itemView = Instantiate(_gridPrefab, _gridContainer);
            itemView.GetComponent<InventoryGridView>().Bind(inventoryGridViewModel);
        }

        private void CreateInventorSubGridView(InventoryGridViewModel inventoryGridViewModel)
        {
            var itemView = Instantiate(_subGridPrefab, _subGridContainer);
            itemView.GetComponent<InventoryGridView>().Bind(inventoryGridViewModel);
        }
    }
}