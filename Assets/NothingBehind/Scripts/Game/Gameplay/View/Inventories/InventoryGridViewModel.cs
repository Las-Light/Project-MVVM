using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class InventoryGridViewModel
    {
        public readonly string GridTypeID;
        public ReactiveMatrix<bool> GridMatrix => _gridMatrix;


        private readonly InventoryGridDataProxy _gridDataProxy;
        private readonly InventoryGridSettings _gridSettings;
        private ReactiveMatrix<bool> _gridMatrix;
        private readonly ObservableDictionary<ItemDataProxy, Vector2Int> _itemPositions = new();

        public InventoryGridViewModel(InventoryGridDataProxy gridDataProxy,
            InventoryGridSettings gridSettings
        )
        {
            GridTypeID = gridDataProxy.GridTypeId;
            _gridDataProxy = gridDataProxy;
            _gridSettings = gridSettings;
            // Подписываемся на изменения и сериализуем
            DeserializeGridMatrix(gridDataProxy);
            _gridMatrix.OnChange.Subscribe(_ => SerializeGrid());
            _itemPositions.ObserveAdd().Subscribe(e => { SerializeAddItemPosition(gridDataProxy, e); });
            _itemPositions.ObserveRemove().Subscribe(e => { SerializeRemoveItemPosition(gridDataProxy, e); });
            _itemPositions.ObserveReplace().Subscribe(e => { SerializeReplaceItemPosition(gridDataProxy, e); });
        }


        public bool AddItem(ItemDataProxy item)
        {
            var position = FindFreePosition(item);
            if (position.HasValue)
            {
                PlaceItem(item, position.Value, isRotated: false);
                return true;
            }

            return false; // Нет свободного места
        }

        public bool AddItem(ItemDataProxy item, Vector2Int position)
        {
            // Проверяем, можно ли добавить предмет на новой позицию
            if (CanPlaceItem(item, position, item.IsRotated.Value))
            {
                // Размещаем предмет на новой позиции
                PlaceItem(item, position, item.IsRotated.Value);
                return true; // Перемещение успешно
            }

            return false; // Нет свободного места
        }

        public bool TryMoveItem(ItemDataProxy item, Vector2Int newPosition)
        {
            // Проверяем, что предмет существует в сетке
            if (!_itemPositions.ContainsKey(item))
                throw new Exception("Item not found in the grid.");

            // Получаем текущую позицию предмета
            var oldPosition = _itemPositions[item];

            // Временно удаляем предмет из сетки
            RemoveItem(item);

            // Проверяем, можно ли разместить предмет на новой позиции
            if (CanPlaceItem(item, newPosition, item.IsRotated.Value))
            {
                // Размещаем предмет на новой позиции
                PlaceItem(item, newPosition, item.IsRotated.Value);
                return true; // Перемещение успешно
            }
            else
            {
                // Если перемещение невозможно, возвращаем предмет на старую позицию
                PlaceItem(item, oldPosition, item.IsRotated.Value);
                return false; // Перемещение не удалось
            }
        }

        public bool SwapItems(ItemDataProxy item1, ItemDataProxy item2)
        {
            // Проверка, что оба предмета существуют в сетке
            if (!_itemPositions.ContainsKey(item1))
            {
                throw new Exception("Item1 not found in the grid.");
            }

            if (!_itemPositions.ContainsKey(item2))
            {
                throw new Exception("Item2 not found in the grid.");
            }

            // Получаем текущие позиции предметов
            var position1 = _itemPositions[item1];
            var position2 = _itemPositions[item2];

            // Временно удаляем оба предмета из сетки
            RemoveItem(item1);
            RemoveItem(item2);

            // Проверяем, что каждый предмет может быть размещён на позиции другого
            bool canPlaceItem1 = CanPlaceItem(item1, position2, item1.IsRotated.Value);
            bool canPlaceItem2 = CanPlaceItem(item2, position1, item2.IsRotated.Value);

            if (canPlaceItem1 && canPlaceItem2)
            {
                // Размещаем предметы на новых позициях
                PlaceItem(item1, position2, item1.IsRotated.Value);
                PlaceItem(item2, position1, item2.IsRotated.Value);
                return true; // Обмен успешен
            }
            else
            {
                // Если обмен невозможен, возвращаем предметы на исходные позиции
                PlaceItem(item1, position1, item1.IsRotated.Value);
                PlaceItem(item2, position2, item2.IsRotated.Value);
                return false; // Обмен не удался
            }
        }

        public bool RotateItem(ItemDataProxy item)
        {
            var position = GetItemPosition(item);
            if (position.HasValue && CanPlaceItem(item, position.Value, !item.IsRotated.Value))
            {
                RemoveItem(item);
                PlaceItem(item, position.Value, !item.IsRotated.Value);
                return true;
            }

            return false; // Невозможно повернуть
        }

        public ItemDataProxy GetItemAtPosition(Vector2Int position)
        {
            foreach (var kvp in _itemPositions)
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

        public Vector2Int? FindFreePosition(ItemDataProxy item)
        {
            for (int y = 0; y < _gridDataProxy.Height.Value - item.Height.Value + 1; y++)
            {
                for (int x = 0; x < _gridDataProxy.Width.Value - item.Width.Value + 1; x++)
                {
                    if (CanPlaceItem(item, new Vector2Int(x, y), isRotated: false))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return null; // Нет свободного места
        }

        public Vector2Int? GetItemPosition(ItemDataProxy item)
        {
            return _itemPositions.TryGetValue(item, out var position) ? position : null;
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
                    if (_gridMatrix.GetValue(i, j)) return false;
                }
            }

            return true;
        }

        private void PlaceItem(ItemDataProxy item, Vector2Int position, bool isRotated)
        {
            if (!CanPlaceItem(item, position, isRotated))
                return;

            int itemWidth = isRotated ? item.Height.Value : item.Width.Value;
            int itemHeight = isRotated ? item.Width.Value : item.Height.Value;

            for (int i = position.x; i < position.x + itemWidth; i++)
            {
                for (int j = position.y; j < position.y + itemHeight; j++)
                {
                    _gridMatrix.SetValue(i, j, true);
                }
            }

            _itemPositions[item] = position;
        }

        private void RemoveItem(ItemDataProxy item)
        {
            if (_itemPositions.TryGetValue(item, out var position))
            {
                int itemWidth = item.Width.Value;
                int itemHeight = item.Height.Value;

                for (int i = position.x; i < position.x + itemWidth; i++)
                {
                    for (int j = position.y; j < position.y + itemHeight; j++)
                    {
                        _gridMatrix.SetValue(i, j, false);
                    }
                }

                _itemPositions.Remove(item);
            }
        }


        // Десериализация

        private void DeserializeGridMatrix(InventoryGridDataProxy gridDataProxy)
        {
            _gridMatrix = new ReactiveMatrix<bool>(gridDataProxy.Width.Value, gridDataProxy.Height.Value);

            // Восстанавливаем двумерный массив из одномерного
            for (int i = 0; i < gridDataProxy.Width.Value; i++)
            {
                for (int j = 0; j < gridDataProxy.Height.Value; j++)
                {
                    _gridMatrix.SetValue(i, j, gridDataProxy.Grid.Value[i * gridDataProxy.Height.Value + j]);
                }
            }

            // Восстанавливаем предметы и их позиции
            _itemPositions.Clear();
            for (int i = 0; i < gridDataProxy.Items.Count; i++)
            {
                var item = gridDataProxy.Items[i];
                _itemPositions[item] = gridDataProxy.Positions[i];
            }
        }

        // Сериализуем предметы и их позиции
        private void SerializeGrid()
        {
            _gridDataProxy.Width.Value = _gridMatrix.GetMatrix().GetLength(0);
            _gridDataProxy.Height.Value = _gridMatrix.GetMatrix().GetLength(1);
            _gridDataProxy.Grid.OnNext(_gridMatrix.GetArray());
        }

        private void SerializeReplaceItemPosition(InventoryGridDataProxy gridDataProxy,
            CollectionReplaceEvent<KeyValuePair<ItemDataProxy, Vector2Int>> e)
        {
            gridDataProxy.Positions[gridDataProxy.Positions.IndexOf(e.OldValue.Value)] = e.NewValue.Value;
        }

        private static void SerializeAddItemPosition(InventoryGridDataProxy gridDataProxy,
            CollectionAddEvent<KeyValuePair<ItemDataProxy, Vector2Int>> e)
        {
            gridDataProxy.Items.Add(e.Value.Key);
            gridDataProxy.Positions.Add(e.Value.Value);
        }

        private void SerializeRemoveItemPosition(InventoryGridDataProxy gridDataProxy,
            CollectionRemoveEvent<KeyValuePair<ItemDataProxy, Vector2Int>> e)
        {
            gridDataProxy.Items.Remove(e.Value.Key);
            gridDataProxy.Positions.Remove(e.Value.Value);
        }
    }
}