using System;
using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class InventoryGridViewModel : IDisposable
    {
        public int OwnerId { get; }
        public string GridTypeID { get; }
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public bool IsSubGrid { get; }

        public List<InventoryGridDataProxy> SubGrids { get; }
        
        public IObservableCollection<ItemDataProxy> Items;
        public IReadOnlyObservableDictionary<int, ItemDataProxy> ItemsMap => _itemsMap;
        public IReadOnlyObservableDictionary<ItemDataProxy, Vector2Int> ItemsPositionsMap => _itemsPositionsMap;

        private readonly ObservableDictionary<ItemDataProxy, Vector2Int> _itemsPositionsMap = new();
        private readonly ObservableDictionary<int, ItemDataProxy> _itemsMap = new();

        private readonly InventoryGridDataProxy _gridDataProxy;
        private readonly InventoryGridSettings _gridSettings;
        private readonly ReactiveMatrix<bool> _gridMatrix;
        private readonly CompositeDisposable _disposables = new();


        public InventoryGridViewModel(InventoryGridDataProxy gridDataProxy,
            InventoryGridSettings gridSettings
        )
        {
            GridTypeID = gridDataProxy.GridTypeId;
            OwnerId = gridDataProxy.OwnerId;
            Height = gridDataProxy.Height;
            Width = gridDataProxy.Width;
            CellSize = gridDataProxy.CellSize;
            IsSubGrid = gridDataProxy.IsSubGrid;
            SubGrids = gridDataProxy.SubGrids;
            _gridDataProxy = gridDataProxy;
            _gridSettings = gridSettings;
            Items = gridDataProxy.Items;

            // Мэпим предметы по Id
            foreach (var itemDataProxy in gridDataProxy.Items)
            {
                _itemsMap[itemDataProxy.Id] = itemDataProxy;
            }

            // Инициализация одномерного массива в кастомный двумерный
            _gridMatrix = new ReactiveMatrix<bool>(gridDataProxy.Width, gridDataProxy.Height);

            // Восстанавливаем двумерный массив из одномерного
            for (int i = 0; i < gridDataProxy.Width; i++)
            {
                for (int j = 0; j < gridDataProxy.Height; j++)
                {
                    _gridMatrix.SetValue(i, j, gridDataProxy.Grid.Value[i * gridDataProxy.Height + j]);
                }
            }

            // Мэпим предметы и их позиции
            for (int i = 0; i < gridDataProxy.Items.Count; i++)
            {
                var item = gridDataProxy.Items[i];
                _itemsPositionsMap[item] = gridDataProxy.Positions[i];
            }

            // Обновляем _itemMap
            _disposables.Add(Items.ObserveAdd().Subscribe(e =>
            {
                var addedItem = e.Value;
                _itemsMap[addedItem.Id] = addedItem;
            }));
            _disposables.Add(Items.ObserveRemove().Subscribe(e =>
            {
                var removedItem = e.Value;
                _itemsMap.Remove(removedItem.Id);
            }));

            // Обновляем данные в GridDataProxy
            _disposables.Add(_gridMatrix.OnChange.Subscribe(_ => gridDataProxy.Grid.OnNext(_gridMatrix.GetArray())));
            _disposables.Add(_itemsPositionsMap.ObserveAdd().Subscribe(e =>
            {
                var addedItemAndPosition = e.Value;
                gridDataProxy.Items.Add(addedItemAndPosition.Key);
                gridDataProxy.Positions.Add(addedItemAndPosition.Value);
            }));
            _disposables.Add(_itemsPositionsMap.ObserveRemove().Subscribe(e =>
            {
                var removedItemAndPosition = e.Value;
                gridDataProxy.Items.Remove(removedItemAndPosition.Key);
                gridDataProxy.Positions.Remove(removedItemAndPosition.Value);
            }));
        }


        // Автодобавление из другой сетки на свободные позиции или добавление количества предметов к уже существующим
        public AddItemsToInventoryGridResult AddItems(ItemDataProxy item, int amount)
        {
            var remainingAmount = amount;
            var itemsAddedToSameItems =
                AddToGridWithSameItems(item, remainingAmount, out remainingAmount);

            //если поместился без остатка, то удаляем его
            if (remainingAmount <= 0)
            {
                return new AddItemsToInventoryGridResult(item.ItemType, item.Id,
                    amount, itemsAddedToSameItems, true, true);
            }

            var itemsAddedToAvailableSlotsAmount =
                AddToFirstAvailableGrid(item, remainingAmount, out remainingAmount);
            var totalAddedItemsAmount = itemsAddedToSameItems + itemsAddedToAvailableSlotsAmount;

            //TODO: рассмотреть если не все предметы влезли в стек (totalAddedItemsAmount != amount)
            var position = FindFreePosition(item);
            if (position.HasValue)
            {
                PlaceItem(item, position.Value, item.IsRotated.Value);
                return new AddItemsToInventoryGridResult(item.ItemType, item.Id,
                    amount,
                    totalAddedItemsAmount, false, true);
            }

            return new AddItemsToInventoryGridResult(item.ItemType, item.Id, amount,
                totalAddedItemsAmount, false, false); // Нет свободного места
        }

        //Добавление предметов из другой сетки по координатам
        public AddItemsToInventoryGridResult AddItems(ItemDataProxy item, Vector2Int position, int amount)
        {
            var remainingAmount = amount;
            var anotherItemAtPosition = GetItemAtPosition(position);

            if (anotherItemAtPosition == null)
            {
                // Проверяем, можно ли разместить предмет на новой позиции
                if (CanPlaceItem(item, position, item.IsRotated.Value))
                {
                    PlaceItem(item, position, item.IsRotated.Value);
                    return new AddItemsToInventoryGridResult(item.ItemType, item.Id, amount,
                        remainingAmount, false, true); // Перемещение успешно
                }
                else
                {
                    return new AddItemsToInventoryGridResult(item.ItemType, item.Id,
                        amount,
                        0, false, false); // Добавить предмет не удалось
                }
            }
            else
            {
                // Если на этой позиции есть предмет то пробуем добавить к нему перемещаемый предмет
                var itemsAddedToSameItems = AddToSameItem(item, anotherItemAtPosition, out remainingAmount);
                // Если предмет поместился без остатка то удаляем его
                if (remainingAmount == 0)
                {
                    return new AddItemsToInventoryGridResult(item.ItemType, item.Id, amount,
                        itemsAddedToSameItems, true, true);
                }
                else
                {
                    // Если все предметы не поместились
                    return new AddItemsToInventoryGridResult(item.ItemType, item.Id, amount,
                        itemsAddedToSameItems, false, false); // Добавить все предметы не удалось
                }
            }
        }

        // Удаление предмета целиком без изменения количества
        public RemoveItemsFromInventoryGridResult RemoveItem(int itemId)
        {
            if (!_itemsMap.TryGetValue(itemId, out var item))
                throw new Exception("Item not found in the grid.");

            if (_itemsPositionsMap.TryGetValue(item, out var position))
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

                _itemsPositionsMap.Remove(item);

                return new RemoveItemsFromInventoryGridResult(item.ItemType, item.Id,
                    item.CurrentStack.Value, true);
            }

            return new RemoveItemsFromInventoryGridResult(item.ItemType, item.Id,
                item.CurrentStack.Value, false);
        }

        //Удаление определенного количества предметов
        public RemoveItemsFromInventoryGridResult RemoveItemAmount(int itemId, int amount)
        {
            if (!_itemsMap.TryGetValue(itemId, out var item))
                throw new Exception("Item not found in the grid.");

            if (_itemsPositionsMap.TryGetValue(item, out var position))
            {
                if (!Has(item, amount))
                {
                    return new RemoveItemsFromInventoryGridResult(item.ItemType, item.Id,
                        amount, false);
                }

                item.CurrentStack.Value -= amount;

                if (item.CurrentStack.Value == 0)
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

                    _itemsPositionsMap.Remove(item);
                }

                return new RemoveItemsFromInventoryGridResult(item.ItemType, item.Id,
                    amount, true);
            }

            return new RemoveItemsFromInventoryGridResult(item.ItemType, item.Id,
                amount, false);
        }

        // Перемещение в одной сетке
        public AddItemsToInventoryGridResult TryMoveItem(int itemId, Vector2Int newPosition, int amount)
        {
            if (!_itemsMap.TryGetValue(itemId, out var item))
                throw new Exception("Item not found in the grid.");

            // Проверяем, что предмет существует в сетке
            if (!_itemsPositionsMap.ContainsKey(item))
                throw new Exception("Item not found in the grid.");

            // Получаем текущую позицию предмета
            var oldPosition = _itemsPositionsMap[item];
            var remainingAmount = amount;
            // Временно удаляем предмет из сетки
            RemoveItem(itemId);
            var anotherItemAtPosition = GetItemAtPosition(newPosition);
            if (anotherItemAtPosition == null)
            {
                // Проверяем, можно ли разместить предмет на новой позиции
                if (CanPlaceItem(item, newPosition, item.IsRotated.Value))
                {
                    PlaceItem(item, newPosition, item.IsRotated.Value);
                    return new AddItemsToInventoryGridResult(item.ItemType, item.Id,
                        amount,
                        remainingAmount, false, true); // Перемещение успешно
                }
                else
                {
                    // Если перемещение невозможно, возвращаем предмет на старую позицию
                    PlaceItem(item, oldPosition, item.IsRotated.Value);
                    return new AddItemsToInventoryGridResult(item.ItemType, item.Id,
                        amount, 0, false, false); // Перемещение не удалось
                }
            }
            else
            {
                // Если на этой позиции есть предмет то пробуем добавить к нему перемещаемый предмет
                var itemsAddedToSameItems = AddToSameItem(item, anotherItemAtPosition, out remainingAmount);

                if (remainingAmount == 0)
                {
                    // Если предмет поместился без остатка
                    return new AddItemsToInventoryGridResult(item.ItemType, item.Id,
                        amount, itemsAddedToSameItems, true, true);
                }
                else
                {
                    // Если все предметы не поместились, возвращаем предмет на старую позицию с остатком который не удалось переместить
                    PlaceItem(item, oldPosition, item.IsRotated.Value);
                    return new AddItemsToInventoryGridResult(item.ItemType, item.Id,
                        amount, itemsAddedToSameItems, false, false); // Переместить все предметы не удалось
                }
            }
        }

        public void SortByType()
        {
            SortItems(item => item.ItemType);
        }

        public void SortByQuantity()
        {
            SortItems(item => -item.CurrentStack.CurrentValue); // Сортировка по убыванию количества
        }

        public void SortByWeight()
        {
            SortItems(item => item.Weight);
        }

        public bool CanPlaceItem(ItemDataProxy item, Vector2Int position, bool isRotated)
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

        private void SortItems(Func<ItemDataProxy, object> sortBy)
        {
            // Временно удаляем все предметы из сетки
            var items = _itemsPositionsMap.Select(kvp => kvp.Key).ToList();

            foreach (var item in items)
            {
                RemoveItem(item.Id);
            }

            // Сортируем предметы
            var sortedItems = items.OrderBy(sortBy).ToList();

            // Размещаем предметы обратно в сетку
            foreach (var item in sortedItems)
            {
                var position = FindFreePosition(item);
                if (position.HasValue)
                {
                    PlaceItem(item, position.Value, item.IsRotated.Value);
                }
            }
        }

        public bool RotateItem(int itemId)
        {
            if (!_itemsMap.TryGetValue(itemId, out var item))
                throw new Exception("Item not found in the grid.");

            if (item.CanRotate)
            {
                var position = GetItemPosition(itemId);
                if (position.HasValue && CanPlaceItem(item, position.Value, !item.IsRotated.Value))
                {
                    RemoveItem(itemId);
                    PlaceItem(item, position.Value, !item.IsRotated.Value);
                    return true;
                }

                return false; // Невозможно повернуть
            }

            return false; // Невозможно повернуть
        }

        public Vector2Int? GetItemPosition(int itemId)
        {
            if (!_itemsMap.TryGetValue(itemId, out var item))
                throw new Exception("Item not found in the grid.");

            return _itemsPositionsMap.TryGetValue(item, out var position) ? position : null;
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

        private Vector2Int? FindFreePosition(ItemDataProxy item)
        {
            for (int y = 0; y < _gridDataProxy.Height - item.Height.Value + 1; y++)
            {
                for (int x = 0; x < _gridDataProxy.Width - item.Width.Value + 1; x++)
                {
                    if (CanPlaceItem(item, new Vector2Int(x, y), isRotated: false))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return null; // Нет свободного места
        }

        private bool Has(ItemDataProxy item, int amount)
        {
            var amountExist = item.CurrentStack.Value;
            return amountExist >= amount;
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

            _itemsPositionsMap[item] = position;
        }


        private int AddToSameItem(ItemDataProxy item, ItemDataProxy anotherItemAtPosition, out int remainingAmount)
        {
            remainingAmount = item.CurrentStack.Value;
            var itemsAddedAmount = 0;
            if (item.IsStackable && anotherItemAtPosition.ItemType == item.ItemType
                                 && anotherItemAtPosition.CurrentStack.Value < anotherItemAtPosition.MaxStackSize)
            {
                var newValue = anotherItemAtPosition.CurrentStack.Value + remainingAmount;

                if (newValue > anotherItemAtPosition.MaxStackSize)
                {
                    remainingAmount = newValue - anotherItemAtPosition.MaxStackSize;
                    var itemsToAddAmount =
                        anotherItemAtPosition.MaxStackSize - anotherItemAtPosition.CurrentStack.Value;
                    itemsAddedAmount += itemsToAddAmount;
                    anotherItemAtPosition.CurrentStack.Value = anotherItemAtPosition.MaxStackSize;

                    if (remainingAmount == 0)
                    {
                        return itemsAddedAmount;
                    }

                    item.CurrentStack.Value = remainingAmount;
                }
                else
                {
                    itemsAddedAmount += remainingAmount;
                    anotherItemAtPosition.CurrentStack.Value = newValue;
                    remainingAmount = 0;

                    return itemsAddedAmount;
                }
            }

            return itemsAddedAmount;
        }

        private int AddToGridWithSameItems(ItemDataProxy item, int amount, out int remainingAmount)
        {
            remainingAmount = amount;
            if (!item.IsStackable)
            {
                return remainingAmount;
            }

            var itemsAddedAmount = 0;

            foreach (var kvp in _itemsPositionsMap)
            {
                if (kvp.Key.ItemType == item.ItemType && kvp.Key.CurrentStack.Value < kvp.Key.MaxStackSize)
                {
                    var newValue = kvp.Key.CurrentStack.Value + remainingAmount;

                    if (newValue > kvp.Key.MaxStackSize)
                    {
                        remainingAmount = newValue - kvp.Key.MaxStackSize;
                        var itemsToAddAmount = kvp.Key.MaxStackSize - kvp.Key.CurrentStack.Value;
                        itemsAddedAmount += itemsToAddAmount;
                        kvp.Key.CurrentStack.Value = kvp.Key.MaxStackSize;

                        if (remainingAmount == 0)
                        {
                            return itemsAddedAmount;
                        }
                    }
                    else
                    {
                        itemsAddedAmount += remainingAmount;
                        kvp.Key.CurrentStack.Value = newValue;
                        remainingAmount = 0;

                        return itemsAddedAmount;
                    }
                }
            }

            return itemsAddedAmount;
        }

        private int AddToFirstAvailableGrid(ItemDataProxy item, int amount, out int remainingAmount)
        {
            remainingAmount = amount;
            if (!item.IsStackable)
            {
                return remainingAmount;
            }

            var itemsAddedAmount = 0;
            var newValue = remainingAmount;
            var itemMaxStackSize = item.MaxStackSize;

            if (newValue > itemMaxStackSize)
            {
                remainingAmount = newValue - itemMaxStackSize;
                var itemsToAddAmount = itemMaxStackSize;
                itemsAddedAmount += itemsToAddAmount;
                item.CurrentStack.Value = itemMaxStackSize;
            }
            else
            {
                itemsAddedAmount += remainingAmount;
                item.CurrentStack.Value = newValue;
                remainingAmount = 0;

                return itemsAddedAmount;
            }

            return itemsAddedAmount;
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}