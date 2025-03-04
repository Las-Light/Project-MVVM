using System;
using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class InventoryGridView : MonoBehaviour
    {
        [SerializeField] private GameObject _cellPrefab; // Префаб для ячеек
        [SerializeField] private GameObject _itemPrefab; // Префаб для предметов
        [SerializeField] private Button _sortByTypeButton;
        [SerializeField] private Button _sortByQuantityButton;
        [SerializeField] private Button _sortByWeightButton;

        public float CellSize; // Размер ячейки в пикселях
        public RectTransform GridContainer; // Контейнер для сетки
        public int Width { get; private set; }
        public int Height { get; private set; }
        public string GridType { get; set; }

        private InventoryGridViewModel _viewModel;
        private ReactiveMatrix<bool> _gridMatrix;
        private IReadOnlyObservableDictionary<ItemDataProxy, Vector2Int> _itemsPositionsMap;
        private IReadOnlyObservableDictionary<int, ItemDataProxy> _itemsMap;
        private readonly Dictionary<ItemDataProxy, GameObject> _itemsViewMap = new Dictionary<ItemDataProxy, GameObject>();
        private GameObject[,] _cells;

        private ReactiveProperty<bool> _isNeedUpdateGrid = new ReactiveProperty<bool>();


        public void Bind(InventoryGridViewModel viewModel)
        {
            _viewModel = viewModel;
            GridType = viewModel.GridTypeID;
            Width = viewModel.Width;
            Height = viewModel.Height;
            CellSize = viewModel.CellSize;
            _gridMatrix = viewModel.GridMatrix;
            _itemsPositionsMap = viewModel.ItemsPositionsMap;
            _itemsMap = viewModel.ItemsMap;

            // Очистка сетки перед инициализацией
            foreach (Transform child in GridContainer) Destroy(child.gameObject);

            // Создание ячеек сетки

            var newGridSize = new Vector2(CellSize * Width, CellSize * Height);
            _cells = new GameObject[Width, Height];

            // Устанавливаем размер GridContainer в соответствии с кол-вом ячеек
            GridContainer.sizeDelta = newGridSize;

            // Устанавливаем размер InventoryGridView
            var viewSize = GetComponent<RectTransform>().sizeDelta;
            viewSize += new Vector2(0, newGridSize.y);
            GetComponent<RectTransform>().sizeDelta = viewSize;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var cell = Instantiate(_cellPrefab, GridContainer);
                    var cellPosition = cell.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(x * CellSize, -y * CellSize);
                    _cells[x, y] = cell;
                    foreach (var kvp in _itemsPositionsMap)
                    {
                        if (kvp.Value == new Vector2Int(x, y))
                        {
                            AddItemView(kvp.Key, cellPosition);
                        }
                    }
                }
            }

            // Устанавливаем ItemView в самый низ иерархии GridContainer для отображения поверх ячеек
            var itemsViews = GetComponentsInChildren<ItemView>();
            foreach (var itemView in itemsViews)
            {
                itemView.transform.SetAsLastSibling();
            }

            // Назначение обработчиков для кнопок сортировки
            _sortByTypeButton.onClick.AddListener(viewModel.SortByType);
            _sortByQuantityButton.onClick.AddListener(viewModel.SortByQuantity);
            _sortByWeightButton.onClick.AddListener(viewModel.SortByWeight);

            _itemsPositionsMap.ObserveAdd().Subscribe(e =>
            {
                var addedItem = e.Value;
                AddItemView(addedItem.Key, new Vector2(addedItem.Value.x * CellSize, -addedItem.Value.y * CellSize));
            });
            _itemsPositionsMap.ObserveRemove().Subscribe(e =>
            {
                var removedItem = e.Value;
                if (_itemsViewMap.TryGetValue(removedItem.Key, out var itemView))
                {
                    _itemsViewMap.Remove(removedItem.Key);
                    Destroy(itemView.gameObject);
                }
            });
            // _itemsPositionsMap.ObserveReplace().Subscribe(e =>
            // {
            //     Debug.Log($"ObserveReplace {GridType}");
            //     var oldItemPos = e.OldValue;
            //     var newItemPos = e.NewValue;
            //     if (_itemsViewMap.TryGetValue(oldItemPos.Key, out var itemView))
            //     {
            //         _itemsViewMap.Remove(oldItemPos.Key);
            //         Destroy(itemView.gameObject);
            //         AddItemView(newItemPos.Key,
            //             new Vector2(newItemPos.Value.x * CellSize, -newItemPos.Value.y * CellSize));
            //     }
            // });
        }
        private void UpdateGridVisual()
        {
            foreach (Transform child in GridContainer)
            {
                if (child.TryGetComponent(out ItemView itemView))
                {
                    Destroy(itemView.gameObject);
                }
            }

            foreach (var kvp in _itemsPositionsMap)
            {
                AddItemView(kvp.Key, new Vector2(kvp.Value.x * CellSize, kvp.Value.y * CellSize));
            }
        }

        private void OnDestroy()
        {
            _sortByTypeButton.onClick.RemoveListener(_viewModel.SortByType);
            _sortByQuantityButton.onClick.RemoveListener(_viewModel.SortByQuantity);
            _sortByWeightButton.onClick.RemoveListener(_viewModel.SortByWeight);
        }

        public InventoryGridViewModel GetGridViewModel()
        {
            return _viewModel;
        }

        // Добавление предмета в UI


        public void UpdateHighlights(ItemDataProxy item, Vector2Int position)
        {
            // Сбрасываем подсветку всех ячеек
            ClearHighlights();

            int itemWidth = item.IsRotated.Value ? item.Height.Value : item.Width.Value;
            int itemHeight = item.IsRotated.Value ? item.Width.Value : item.Height.Value;

            for (int x = 0; x < _cells.GetLength(0); x++)
            {
                for (int y = 0; y < _cells.GetLength(1); y++)
                {
                    bool isHighlighted = x >= position.x && x < position.x + itemWidth &&
                                         y >= position.y && y < position.y + itemHeight;

                    if (isHighlighted)
                    {
                        bool canPlace = CanPlaceItem(item, position, item.IsRotated.Value);
                        _cells[x, y].GetComponent<Image>().color = canPlace ? Color.green : Color.red;
                    }
                }
            }
        }

        public void ClearHighlights()
        {
            for (int x = 0; x < _cells.GetLength(0); x++)
            {
                for (int y = 0; y < _cells.GetLength(1); y++)
                {
                    _cells[x, y].GetComponent<Image>().color = Color.white;
                }
            }
        }

        private bool CanPlaceItem(ItemDataProxy item, Vector2Int position, bool isRotated)
        {
            int itemWidth = isRotated ? item.Height.Value : item.Width.Value;
            int itemHeight = isRotated ? item.Width.Value : item.Height.Value;

            if (position.x < 0 || position.y < 0 || position.x + itemWidth > _gridMatrix.GetMatrix().GetLength(0) ||
                position.y + itemHeight > _gridMatrix.GetMatrix().GetLength(1))
                return false;

            for (int i = position.x; i < position.x + itemWidth; i++)
            {
                for (int j = position.y; j < position.y + itemHeight; j++)
                {
                    if (_gridMatrix.GetValue(i, j))
                    {
                        if (GetItemAtPosition(new Vector2Int(i, j)) != item)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private ItemDataProxy GetItemAtPosition(Vector2Int position)
        {
            foreach (var kvp in _itemsPositionsMap)
            {
                var item = kvp.Key;
                var itemPosition = kvp.Value;
                int itemWidth = item.IsRotated.Value ? item.Height.Value : item.Width.Value;
                int itemHeight = item.IsRotated.Value ? item.Width.Value : item.Height.Value;

                // Проверяем, находится ли позиция в пределах предмета
                if (position.x >= itemPosition.x && position.x < itemPosition.x + itemWidth &&
                    position.y >= itemPosition.y && position.y < itemPosition.y + itemHeight)
                {
                    return item;
                }
            }

            return null; // Позиция пуста
        }

        private void AddItemView(ItemDataProxy itemData, Vector2 cellPosition)
        {
            var itemView = Instantiate(_itemPrefab, GridContainer.transform);
            itemView.GetComponent<RectTransform>().anchoredPosition = cellPosition;
            itemView.GetComponent<ItemView>().Initialize(itemData, CellSize);
            _itemsViewMap[itemData] = itemView;
        }
    }
}