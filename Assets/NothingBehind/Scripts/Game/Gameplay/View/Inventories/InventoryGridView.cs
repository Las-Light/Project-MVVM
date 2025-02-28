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
        
        public float CellSize = 50f; // Размер ячейки в пикселях
        public RectTransform GridContainer; // Контейнер для сетки
        public int Width { get; private set; }
        public int Height { get; private set; }

        private InventoryGridViewModel _viewModel;
        private ObservableDictionary<ItemDataProxy, Vector2Int> _itemsPosition;
        private IObservableCollection<ItemDataProxy> _items;
        private readonly List<ItemDataProxy> _itemData = new List<ItemDataProxy>();
        private GameObject[,] _cells;

        public void Bind(InventoryGridViewModel viewModel)
        {
            _viewModel = viewModel;
            Width = _viewModel.Width;
            Height = _viewModel.Height;
            _itemsPosition = viewModel.ItemPositions;

            // Очистка сетки перед инициализацией
            foreach (Transform child in GridContainer) Destroy(child.gameObject);

            // Создание ячеек сетки

            var newGridSize = new Vector2(CellSize * Width, CellSize * Height);
            _cells = new GameObject[Width, Height];

            GetComponent<RectTransform>().sizeDelta = newGridSize;
            GridContainer.sizeDelta = newGridSize;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var cell = Instantiate(_cellPrefab, GridContainer);
                    var cellPosition = cell.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(x * CellSize, -y * CellSize);
                    _cells[x, y] = cell;
                    foreach (var kvp in _itemsPosition)
                    {
                        if (kvp.Value == new Vector2Int(x, y) && !_itemData.Contains(kvp.Key))
                        {
                            _itemData.Add(kvp.Key);
                            AddItemView(kvp.Key, cellPosition);
                        }
                    }
                }
            }

            _itemsPosition.ObserveRemove().Subscribe(e =>
            {
                _itemData.Remove(e.Value.Key);
            });
        }

        public InventoryGridViewModel GetGridViewModel()
        {
            return _viewModel;
        }

        // Добавление предмета в UI
        private void AddItemView(ItemDataProxy itemData, Vector2 cellPosition)
        {
            var itemView = Instantiate(_itemPrefab, transform);
            itemView.GetComponent<RectTransform>().anchoredPosition = cellPosition;
            itemView.GetComponent<ItemView>().Initialize(itemData, CellSize);
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
    }
}