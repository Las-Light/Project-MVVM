using System;
using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories
{
    public class InventoryGridViewModel : IDisposable
    {
        public int GridId { get; }
        public InventoryGridType GridType { get; }
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public bool IsSubGrid { get; }

        public IReadOnlyObservableDictionary<int, Item> ItemsMap => _itemsMap;
        public IReadOnlyObservableDictionary<Item, Vector2Int> ItemsPositionsMap => _itemsPositionsMap;

        private readonly ObservableDictionary<Item, Vector2Int> _itemsPositionsMap;
        private readonly ObservableDictionary<int, Item> _itemsMap = new();
        private readonly ObservableDictionary<int, ItemViewModel> _itemViewModelsMap = new();
        private readonly InventoryGrid _inventoryGrid;
        private readonly InventoryGridSettings _gridSettings;
        private readonly ReactiveMatrix<bool> _gridMatrix;
        private readonly CompositeDisposable _disposables = new();


        public InventoryGridViewModel(InventoryGrid inventoryGrid,
            InventoryGridSettings gridSettings,
            ItemsSettings itemsSettings,
            ObservableDictionary<int, Item> allInventoryItems)
        {
            GridType = inventoryGrid.GridType;
            GridId = inventoryGrid.GridId;
            Height = inventoryGrid.Height;
            Width = inventoryGrid.Width;
            CellSize = inventoryGrid.CellSize;
            IsSubGrid = inventoryGrid.IsSubGrid;
            _inventoryGrid = inventoryGrid;
            _gridSettings = gridSettings;
            _itemsPositionsMap = inventoryGrid.ItemsPositionsMap;

            // Инициализация одномерного массива в кастомный двумерный
            _gridMatrix = new ReactiveMatrix<bool>(inventoryGrid.Width, inventoryGrid.Height);

            // Восстанавливаем двумерный массив из одномерного
            for (int i = 0; i < inventoryGrid.Width; i++)
            {
                for (int j = 0; j < inventoryGrid.Height; j++)
                {
                    _gridMatrix.SetValue(i, j, inventoryGrid.Grid.Value[i * inventoryGrid.Height + j]);
                }
            }

            // Мэпим предметы
            foreach (var kvp in _itemsPositionsMap)
            {
                _itemsMap[kvp.Key.Id] = kvp.Key;
                CreateItemViewModel(kvp.Key, itemsSettings);
            }

            // Обновляем данные в GridDataProxy
            _disposables.Add(_gridMatrix.OnChange.Subscribe(_ => inventoryGrid.Grid.OnNext(_gridMatrix.GetArray())));

            //Обновляем _itemsMap
            _disposables.Add(_itemsPositionsMap.ObserveAdd().Subscribe(kvp =>
            {
                var addedItem = kvp.Value.Key;
                _itemsMap[addedItem.Id] = addedItem;
                allInventoryItems[addedItem.Id] = addedItem;
                CreateItemViewModel(addedItem, itemsSettings);
            }));
            _disposables.Add(_itemsPositionsMap.ObserveRemove().Subscribe(kvp =>
            {
                var removedItem = kvp.Value.Key;
                _itemsMap.Remove(removedItem.Id);
                allInventoryItems.Remove(removedItem.Id);
                RemoveItemViewModel(removedItem);
            }));
        }


        // Автодобавление из другой сетки на свободные позиции или добавление количества предметов к уже существующим

        public AddItemAmountResult AddItems(Item item, int amount)
        {
            var remainingAmount = amount;
            var itemsAddedToSameItems =
                AddToGridWithSameItems(item, remainingAmount, out remainingAmount);

            //если поместился без остатка, то удаляем его
            if (remainingAmount <= 0)
            {
                return new AddItemAmountResult(item.ItemType, item.Id,
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
                return new AddItemAmountResult(item.ItemType, item.Id,
                    amount,
                    totalAddedItemsAmount, false, true);
            }


            return new AddItemAmountResult(item.ItemType, item.Id, amount,
                totalAddedItemsAmount, false, false); // Нет свободного места
        }


        //Добавление предметов из другой сетки по координатам

        public AddItemAmountResult AddItems(Item item, Vector2Int position, int amount)
        {
            var remainingAmount = amount;
            var anotherItemAtPosition = GetItemAtPosition(position);

            if (anotherItemAtPosition == null)
            {
                // Проверяем, можно ли разместить предмет на новой позиции
                if (CanPlaceItem(item, position, item.IsRotated.Value))
                {
                    PlaceItem(item, position, item.IsRotated.Value);
                    return new AddItemAmountResult(item.ItemType, item.Id, amount,
                        remainingAmount, true, true); // Перемещение успешно
                }
                else
                {
                    return new AddItemAmountResult(item.ItemType, item.Id,
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
                    return new AddItemAmountResult(item.ItemType, item.Id, amount,
                        itemsAddedToSameItems, true, true);
                }
                else
                {
                    // Если все предметы не поместились
                    return new AddItemAmountResult(item.ItemType, item.Id, amount,
                        itemsAddedToSameItems, false, false); // Добавить все предметы не удалось
                }
            }
        }


        // Удаление предмета целиком без изменения количества

        public RemoveItemAmountResult RemoveItem(int itemId)
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

                return new RemoveItemAmountResult(item.ItemType, item.Id,
                    item.CurrentStack.Value, true);
            }

            return new RemoveItemAmountResult(item.ItemType, item.Id,
                item.CurrentStack.Value, false);
        }


        //Удаление определенного количества предметов

        public RemoveItemAmountResult RemoveItemAmount(int itemId, int amount)
        {
            if (!_itemsMap.TryGetValue(itemId, out var item))
                throw new Exception("Item not found in the grid.");

            if (_itemsPositionsMap.TryGetValue(item, out var position))
            {
                if (!Has(item, amount))
                {
                    return new RemoveItemAmountResult(item.ItemType, item.Id,
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

                return new RemoveItemAmountResult(item.ItemType, item.Id,
                    amount, true);
            }

            return new RemoveItemAmountResult(item.ItemType, item.Id,
                amount, false);
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

        public bool CanPlaceItem(Item item, Vector2Int position, bool isRotated)
        {
            if (!CheckForAttachInside(item)) return false;

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

        private bool CheckForAttachInside(Item item)
        {
            if (item is GridItem gridItem)
            {
                if (_inventoryGrid == gridItem.Grid.Value)
                {
                    return false;
                }

                if (gridItem.Grid.Value is InventoryGridWithSubGrid subGrid)
                {
                    foreach (var grid in subGrid.SubGrids)
                    {
                        if (grid == _inventoryGrid)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void SortItems(Func<Item, object> sortBy)
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

        public Item GetItemAtPosition(Vector2Int position)
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

        private Vector2Int? FindFreePosition(Item item)
        {
            for (int y = 0; y < _inventoryGrid.Height - item.Height.Value + 1; y++)
            {
                for (int x = 0; x < _inventoryGrid.Width - item.Width.Value + 1; x++)
                {
                    if (CanPlaceItem(item, new Vector2Int(x, y), isRotated: false))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return null; // Нет свободного места
        }

        private bool Has(Item item, int amount)
        {
            var amountExist = item.CurrentStack.Value;
            return amountExist >= amount;
        }


        private void PlaceItem(Item item, Vector2Int position, bool isRotated)
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


        private int AddToSameItem(Item item, Item anotherItemAtPosition, out int remainingAmount)
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

        private int AddToGridWithSameItems(Item item, int amount, out int remainingAmount)
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

        private int AddToFirstAvailableGrid(Item item, int amount, out int remainingAmount)
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

        private void CreateItemViewModel(Item item, ItemsSettings itemsSettings)
        {
            var itemSettings = itemsSettings.Items.FirstOrDefault(itemConfig => itemConfig.ItemType == item.ItemType);
            if (itemSettings == null)
            {
                Debug.LogError($"ItemSettings with type {item.ItemType} not found");
            }

            var itemViewModel = new ItemViewModel(item, itemSettings);
            _itemViewModelsMap[item.Id] = itemViewModel;
        }

        private void RemoveItemViewModel(Item item)
        {
            if (_itemViewModelsMap.TryGetValue(item.Id, out _))
            {
                _itemViewModelsMap.Remove(item.Id);
            }
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}