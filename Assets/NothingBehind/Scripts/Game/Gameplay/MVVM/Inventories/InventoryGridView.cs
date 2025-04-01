using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories
{
    public class InventoryGridView : MonoBehaviour, IView
    {
        [SerializeField] private GameObject _cellPrefab; // Префаб для ячеек
        [SerializeField] private GameObject _itemPrefab; // Префаб для предметов
        [SerializeField] private Button _sortByTypeButton;
        [SerializeField] private Button _sortByQuantityButton;
        [SerializeField] private Button _sortByWeightButton;

        public RectTransform GridContainer; // Контейнер для сетки


        public int GridId { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public InventoryGridType GridType { get; private set; }
        public float CellSize; // Размер ячейки в пикселях
        public Dictionary<Item, ItemView> ItemsViewMap => _itemsViewMap;


        private IReadOnlyObservableDictionary<Item, Vector2Int> _itemsPositionsMap;
        private List<ItemView> _itemViews;
        private readonly Dictionary<Item, ItemView> _itemsViewMap = new Dictionary<Item, ItemView>();
        private readonly CompositeDisposable _disposables = new();

        private Image[,] _cellsImage;
        private InventoryGridViewModel _viewModel;
        private RectTransform _inventoryGridViewRectTransform;


        public void Bind(InventoryGridViewModel viewModel, List<ItemView> itemViews)
        {
            _viewModel = viewModel;
            GridId = viewModel.GridId;
            GridType = viewModel.GridType;
            Width = viewModel.Width;
            Height = viewModel.Height;
            CellSize = viewModel.CellSize;
            _itemsPositionsMap = viewModel.ItemsPositionsMap;
            _itemViews = itemViews;

            // Очистка сетки перед инициализацией
            foreach (Transform child in GridContainer) Destroy(child.gameObject);

            // Создание двумерного массива ячеек сетки
            _cellsImage = new Image[Width, Height];

            // Устанавливаем размер GridContainer в соответствии с кол-вом ячеек
            var newGridSize = new Vector2(CellSize * Width, CellSize * Height);
            GridContainer.sizeDelta = newGridSize;

            // Устанавливаем размер InventoryGridView в учетом с размера GridContainer
            _inventoryGridViewRectTransform = GetComponent<RectTransform>();
            _inventoryGridViewRectTransform.sizeDelta =
                newGridSize; // TODO: Надо откорректировать на размер кнопок сортировки
            // viewSize += new Vector2(0, newGridSize.y);
            // GetComponent<RectTransform>().sizeDelta = viewSize;

            // Создаем ячейки сетки и создаем вьюхи на позициях ячеек
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var cell = Instantiate(_cellPrefab, GridContainer);
                    var cellPosition = cell.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(x * CellSize, -y * CellSize);
                    _cellsImage[x, y] = cell.GetComponent<Image>();
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
            foreach (var kvp in _itemsViewMap)
            {
                var itemView = kvp.Value;
                itemView.transform.SetAsLastSibling();
            }

            // Назначение обработчиков для кнопок сортировки
            _sortByTypeButton?.onClick.AddListener(viewModel.SortByType);
            _sortByQuantityButton?.onClick.AddListener(viewModel.SortByQuantity);
            _sortByWeightButton?.onClick.AddListener(viewModel.SortByWeight);


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
            _sortByTypeButton?.onClick.RemoveListener(_viewModel.SortByType);
            _sortByQuantityButton?.onClick.RemoveListener(_viewModel.SortByQuantity);
            _sortByWeightButton?.onClick.RemoveListener(_viewModel.SortByWeight);

            _disposables.Dispose();
        }

        public AddItemsToInventoryGridResult AddItems(Item item, int amount)
        {
            return _viewModel.AddItems(item, amount);
        }

        public AddItemsToInventoryGridResult AddItems(Item item, Vector2Int position, int amount)
        {
            return _viewModel.AddItems(item, position, amount);
        }

        public RemoveItemsFromInventoryGridResult RemoveItem(int itemId)
        {
            return _viewModel.RemoveItem(itemId);
        }

        public RemoveItemsFromInventoryGridResult RemoveItemAmount(int itemId, int amount)
        {
            return _viewModel.RemoveItemAmount(itemId, amount);
        }

        public Vector2Int? GetItemPosition(int itemId)
        {
            return _viewModel.GetItemPosition(itemId);
        }

        public Item GetItemAtPosition(Vector2Int position)
        {
            return _viewModel.GetItemAtPosition(position);
        }

        public void UpdateHighlights(Item item, Vector2Int position)
        {
            // Сбрасываем подсветку всех ячеек
            ClearHighlights();

            int itemWidth = item.IsRotated.Value ? item.Height.Value : item.Width.Value;
            int itemHeight = item.IsRotated.Value ? item.Width.Value : item.Height.Value;

            for (int x = 0; x < _cellsImage.GetLength(0); x++)
            {
                for (int y = 0; y < _cellsImage.GetLength(1); y++)
                {
                    bool isHighlighted = x >= position.x && x < position.x + itemWidth &&
                                         y >= position.y && y < position.y + itemHeight;

                    if (isHighlighted)
                    {
                        bool canPlace = _viewModel.CanPlaceItem(item, position, item.IsRotated.Value);
                        _cellsImage[x, y].color = canPlace ? Color.green : Color.red;
                    }
                }
            }
        }

        public void ClearHighlights()
        {
            for (int x = 0; x < _cellsImage.GetLength(0); x++)
            {
                for (int y = 0; y < _cellsImage.GetLength(1); y++)
                {
                    _cellsImage[x, y].color = Color.white;
                }
            }
        }

        // Добавление предмета в UI
        private void AddItemView(Item item, Vector2 cellPosition)
        {
            var itemGameObject = Instantiate(_itemPrefab, GridContainer.transform);
            itemGameObject.GetComponent<RectTransform>().anchoredPosition = cellPosition;
            var itemView = itemGameObject.GetComponent<ItemView>();
            _itemViews.Add(itemView);
            itemView.Bind(item, CellSize, _itemViews);
            _itemsViewMap[item] = itemView;
        }
    }
}