using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;
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

        private IObservableCollection<InventoryGridViewModel> _inventoryGridViewModels;
        private readonly Dictionary<InventoryGridViewModel, InventoryGridView> _gridViewsMap = new ();
        private readonly CompositeDisposable _disposables = new ();
        public void Bind(InventoryViewModel viewModel)
        {
            _inventoryViewModel = viewModel;
            _inventoryGridViewModels = viewModel.AllInventoryGrids;
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

            _disposables.Add(_inventoryGridViewModels.ObserveAdd().Subscribe(e =>
            {
                if (e.Value.IsSubGrid)
                {
                    CreateInventorSubGridView(e.Value);
                }
                else
                {
                    CreateInventorGridView(e.Value);
                }
            }));
            _disposables.Add(_inventoryGridViewModels.ObserveRemove().Subscribe(e=>RemoveGridView(e.Value)));

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

        private void CreateInventorGridView(InventoryGridViewModel inventoryGridViewModel)
        {
            var grid = Instantiate(_gridPrefab, _gridContainer);
            var gridView = grid.GetComponent<InventoryGridView>();
            grid.name = new string($"{inventoryGridViewModel.GridType} {inventoryGridViewModel.GridId}");
            gridView.Bind(inventoryGridViewModel);
            _gridViewsMap[inventoryGridViewModel] = gridView;
        }

        private void CreateInventorSubGridView(InventoryGridViewModel inventoryGridViewModel)
        {
            var subGrid = Instantiate(_subGridPrefab, _subGridContainer);
            var subGridView = subGrid.GetComponent<InventoryGridView>();
            subGrid.name = new string($"{inventoryGridViewModel.GridType} {inventoryGridViewModel.GridId}");
            subGridView.Bind(inventoryGridViewModel);
            _gridViewsMap[inventoryGridViewModel] = subGridView;
        }

        private void RemoveGridView(InventoryGridViewModel inventoryGridViewModel)
        {
            _gridViewsMap.TryGetValue(inventoryGridViewModel, out var gridView);
            if (gridView != null)
            {
                Destroy(gridView.gameObject);
            }
        }
    }
}