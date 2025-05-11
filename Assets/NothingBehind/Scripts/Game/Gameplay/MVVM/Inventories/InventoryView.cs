using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private GameObject _gridPrefab;
        [SerializeField] private GameObject _subGridPrefab;
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private RectTransform _subGridContainer;

        public List<InventoryGridView> InventoryGridViews;
        private int OwnerId { get; set; }

        private InventoryViewModel _inventoryViewModel;
        private int _gridIndex;

        private IObservableCollection<InventoryGridViewModel> _inventoryGridViewModels;
        private List<ItemView> _itemViews;
        private readonly Dictionary<InventoryGridViewModel, InventoryGridView> _gridViewsMap = new();
        private readonly CompositeDisposable _disposables = new();

        public void Bind(InventoryViewModel viewModel,
            List<ItemView> itemViews)
        {
            _inventoryViewModel = viewModel;
            _inventoryGridViewModels = viewModel.AllInventoryGrids;
            _itemViews = itemViews;
            OwnerId = viewModel.OwnerId;
            foreach (var inventoryGridViewModel in _inventoryGridViewModels)
            {
                if (inventoryGridViewModel.IsSubGrid)
                {
                    CreateInventorSubGridView(inventoryGridViewModel, _itemViews);
                }
                else
                {
                    CreateInventorGridView(inventoryGridViewModel, _itemViews);
                }
            }

            _disposables.Add(_inventoryGridViewModels.ObserveAdd().Subscribe(e =>
            {
                if (e.Value.IsSubGrid)
                {
                    CreateInventorSubGridView(e.Value, itemViews);
                }
                else
                {
                    CreateInventorGridView(e.Value, itemViews);
                }
            }));
            _disposables.Add(_inventoryGridViewModels.ObserveRemove().Subscribe(e => { RemoveGridView(e.Value); }));

            // Задаем размер инвентаря в соответствии с размером экрана
            var viewScreenSize = new Vector2(Screen.width / 3, Screen.height);
            transform.parent.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            _gridContainer.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void CreateInventorGridView(InventoryGridViewModel inventoryGridViewModel,
            List<ItemView> itemViews)
        {
            var grid = Instantiate(_gridPrefab, _gridContainer);
            var gridView = grid.GetComponent<InventoryGridView>();
            _gridIndex++;
            grid.name = new string($"{inventoryGridViewModel.GridType} {inventoryGridViewModel.GridId}");
            gridView.Bind(inventoryGridViewModel, itemViews, _gridIndex);
            _gridViewsMap[inventoryGridViewModel] = gridView;
            InventoryGridViews.Add(gridView);
        }

        private void CreateInventorSubGridView(InventoryGridViewModel inventoryGridViewModel,
            List<ItemView> itemViews)
        {
            var subGrid = Instantiate(_subGridPrefab, _subGridContainer);
            var subGridView = subGrid.GetComponent<InventoryGridView>();
            _gridIndex++;
            subGrid.name = new string($"{inventoryGridViewModel.GridType} {inventoryGridViewModel.GridId}");
            subGridView.Bind(inventoryGridViewModel, itemViews, _gridIndex);
            _gridViewsMap[inventoryGridViewModel] = subGridView;
            InventoryGridViews.Add(subGridView);
        }

        private void RemoveGridView(InventoryGridViewModel inventoryGridViewModel)
        {
            _gridViewsMap.TryGetValue(inventoryGridViewModel, out var gridView);
            if (gridView != null)
            {
                InventoryGridViews.Remove(gridView);
                Destroy(gridView.gameObject);
            }
        }
    }
}