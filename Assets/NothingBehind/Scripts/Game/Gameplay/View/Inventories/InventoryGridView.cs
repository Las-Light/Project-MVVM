using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Inventory;
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
        public string GridTypeId { get; private set; }

        private InventoryGridViewModel _viewModel;
        
        private IReadOnlyObservableDictionary<ItemDataProxy, Vector2Int> _itemsPositionsMap;
        private readonly Dictionary<ItemDataProxy, GameObject> _itemsViewMap = new Dictionary<ItemDataProxy, GameObject>();
        private readonly CompositeDisposable _disposables = new ();
        
        private GameObject[,] _cells;

        public void Bind(InventoryGridViewModel viewModel)
        {
            _viewModel = viewModel;
            GridTypeId = viewModel.GridTypeID;
            Width = viewModel.Width;
            Height = viewModel.Height;
            CellSize = viewModel.CellSize;
            _itemsPositionsMap = viewModel.ItemsPositionsMap;

            // Очистка сетки перед инициализацией
            foreach (Transform child in GridContainer) Destroy(child.gameObject);

            // Создание двумерного массива ячеек сетки
            _cells = new GameObject[Width, Height];

            // Устанавливаем размер GridContainer в соответствии с кол-вом ячеек
            var newGridSize = new Vector2(CellSize * Width, CellSize * Height);
            GridContainer.sizeDelta = newGridSize;

            // Устанавливаем размер InventoryGridView в учетом с размера GridContainer
            var viewSize = GetComponent<RectTransform>().sizeDelta;
            viewSize += new Vector2(0, newGridSize.y);
            GetComponent<RectTransform>().sizeDelta = viewSize;

            // Создаем ячейки сетки и создаем вьюхи на позициях ячеек
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

            // Подписываемся на добавление и удаление предметов для создания и удаления вьюх на сетке
            // И добавляем подписку в CompositeDispose для отписки при удалении вьюхи
            _disposables.Add(_itemsPositionsMap.ObserveDictionaryAdd().Subscribe(e =>
            {
                AddItemView(e.Key, new Vector2(e.Value.x * CellSize, -e.Value.y * CellSize));
            }));
            _disposables.Add(_itemsPositionsMap.ObserveDictionaryRemove().Subscribe(e =>
            {
                if (_itemsViewMap.TryGetValue(e.Key, out var itemView))
                {
                    _itemsViewMap.Remove(e.Key);
                    Destroy(itemView.gameObject);
                }
            }));
        }

        private void OnDestroy()
        {
            _sortByTypeButton.onClick.RemoveListener(_viewModel.SortByType);
            _sortByQuantityButton.onClick.RemoveListener(_viewModel.SortByQuantity);
            _sortByWeightButton.onClick.RemoveListener(_viewModel.SortByWeight);
            _disposables.Dispose();
        }

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
                        bool canPlace = _viewModel.CanPlaceItem(item, position, item.IsRotated.Value);
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

        // Добавление предмета в UI
        private void AddItemView(ItemDataProxy itemData, Vector2 cellPosition)
        {
            var itemView = Instantiate(_itemPrefab, GridContainer.transform);
            itemView.GetComponent<RectTransform>().anchoredPosition = cellPosition;
            itemView.GetComponent<ItemView>().Initialize(itemData, CellSize);
            _itemsViewMap[itemData] = itemView;
        }
    }
}