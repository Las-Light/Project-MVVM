using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Items;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.MVVM.Inventories
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
    
            // Инициализация сеток
            foreach (var inventoryGridViewModel in _inventoryGridViewModels)
            {
                CreateGridView(inventoryGridViewModel, itemViews);
            }

            // Подписки на изменения
            _disposables.Add(_inventoryGridViewModels.ObserveAdd().Subscribe(e => 
            {
                CreateGridView(e.Value, itemViews);
                UpdateGridIndices(); // Обновляем индексы после добавления
            }));
    
            _disposables.Add(_inventoryGridViewModels.ObserveRemove().Subscribe(e => 
            {
                RemoveGridView(e.Value);
                UpdateGridIndices(); // Обновляем индексы после удаления
            }));

            // Настройка размеров
            UpdateViewSize();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void UpdateGridIndices()
        {
            // Сортируем все сетки по GridId для сохранения порядка
            var sortedGrids = _inventoryGridViewModels
                .OrderBy(g => g.GridId)
                .ToList();

            // Обновляем индексы
            for (int i = 0; i < sortedGrids.Count; i++)
            {
                var viewModel = sortedGrids[i];
                if (_gridViewsMap.TryGetValue(viewModel, out var gridView))
                {
                    gridView.UpdateGridIndex(i);
                }
            }

            // Обновляем локальный счетчик
            _gridIndex = sortedGrids.Count;
        }
        
        private void CreateGridView(InventoryGridViewModel viewModel, List<ItemView> itemViews)
        {
            GameObject prefab = viewModel.IsSubGrid ? _subGridPrefab : _gridPrefab;
            Transform parent = viewModel.IsSubGrid ? _subGridContainer : _gridContainer;
    
            var grid = Instantiate(prefab, parent);
            var gridView = grid.GetComponent<InventoryGridView>();
            grid.name = $"{viewModel.GridType} {viewModel.GridId}";
    
            // Временный индекс, будет обновлен в UpdateGridIndices
            gridView.Bind(viewModel, itemViews, -1); 
    
            _gridViewsMap[viewModel] = gridView;
            InventoryGridViews.Add(gridView);
    
            // Первоначальное обновление индекса
            UpdateGridIndices();
        }

        private void UpdateViewSize()
        {
            var viewScreenSize = new Vector2(Screen.width / 10 * 3, Screen.height);
            transform.parent.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            _gridContainer.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
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