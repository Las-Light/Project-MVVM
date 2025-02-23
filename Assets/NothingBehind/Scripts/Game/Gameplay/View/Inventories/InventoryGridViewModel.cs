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
    public class InventoryGridViewModel
    {
        public int OwnerId { get; }
        public string GridTypeID { get; }
        public ReactiveMatrix<bool> GridMatrix => _gridMatrix;

        private readonly InventoryGridDataProxy _gridDataProxy;
        private readonly InventoryGridSettings _gridSettings;
        private ReactiveMatrix<bool> _gridMatrix;
        public readonly ObservableDictionary<ItemDataProxy, Vector2Int> _itemPositions = new();


        public InventoryGridViewModel(InventoryGridDataProxy gridDataProxy,
            InventoryGridSettings gridSettings
        )
        {
            GridTypeID = gridDataProxy.GridTypeId;
            OwnerId = gridDataProxy.OwnerId;
            _gridDataProxy = gridDataProxy;
            _gridSettings = gridSettings;
            // Подписываемся на изменения и сериализуем
            InitializeGrid(gridDataProxy);
            _gridMatrix.OnChange.Subscribe(_ => SerializeGrid());
            _itemPositions.ObserveAdd().Subscribe(e => { SerializeAddItemPosition(gridDataProxy, e); });
            _itemPositions.ObserveRemove().Subscribe(e => { SerializeRemoveItemPosition(gridDataProxy, e); });
            _itemPositions.ObserveReplace().Subscribe(e => { SerializeReplaceItemPosition(gridDataProxy, e); });
        }


        // Автодобавление из другой сетки на свободные позиции или добавление количества предметов к уже существующим
        public AddItemsToInventoryGridResult AddItems(ItemDataProxy item, int amount)
        {
            var remainingAmount = amount;
            var itemsAddedToSameItems =
                AddToGridWithSameItems(item, remainingAmount, out remainingAmount);

            if (remainingAmount <= 0)
            {
                return new AddItemsToInventoryGridResult(item.ItemType,
                    amount, itemsAddedToSameItems, true);
            }

            var itemsAddedToAvailableSlotsAmount =
                AddToFirstAvailableGrid(item, remainingAmount, out remainingAmount);
            var totalAddedItemsAmount = itemsAddedToSameItems + itemsAddedToAvailableSlotsAmount;

            var position = FindFreePosition(item);
            if (position.HasValue)
            {
                PlaceItem(item, position.Value, item.IsRotated.Value);
                return new AddItemsToInventoryGridResult(item.ItemType,
                    amount,
                    totalAddedItemsAmount, true);
            }

            return new AddItemsToInventoryGridResult(item.ItemType, amount,
                totalAddedItemsAmount, false); // Нет свободного места
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
                    return new AddItemsToInventoryGridResult(item.ItemType, amount,
                        remainingAmount, true); // Перемещение успешно
                }
                else
                {
                    return new AddItemsToInventoryGridResult(item.ItemType,
                        amount,
                        0, false); // Добавить предмет не удалось
                }
            }
            else
            {
                // Если на этой позиции есть предмет то пробуем добавить к нему перемещаемый предмет
                var itemsAddedToSameItems = AddToSameItem(item, anotherItemAtPosition, out remainingAmount);
                // Если предмет поместился без остатка то удаляем его
                if (remainingAmount == 0)
                {
                    return new AddItemsToInventoryGridResult(item.ItemType, amount,
                        itemsAddedToSameItems, true);
                }
                else
                {
                    // Если все предметы не поместились
                    return new AddItemsToInventoryGridResult(item.ItemType, amount,
                        itemsAddedToSameItems, false); // Добавить все предметы не удалось
                }
            }
        }

        // Удаление предмета целиком без изменения количества
        public RemoveItemsFromInventoryGridResult RemoveItem(ItemDataProxy item)
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

                return new RemoveItemsFromInventoryGridResult(item.ItemType,
                    item.CurrentStack.Value, true);
            }

            return new RemoveItemsFromInventoryGridResult(item.ItemType,
                item.CurrentStack.Value, false);
        }

        //Удаление определенного количества предметов
        public RemoveItemsFromInventoryGridResult RemoveItemAmount(ItemDataProxy item, int amount)
        {
            if (_itemPositions.TryGetValue(item, out var position))
            {
                if (!Has(item, amount))
                {
                    return new RemoveItemsFromInventoryGridResult(item.ItemType,
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

                    _itemPositions.Remove(item);
                }

                return new RemoveItemsFromInventoryGridResult(item.ItemType,
                    amount, true);
            }

            return new RemoveItemsFromInventoryGridResult(item.ItemType,
                amount, false);
        }

        // Перемещение в одной сетке
        public AddItemsToInventoryGridResult TryMoveItem(ItemDataProxy item, Vector2Int newPosition, int amount)
        {
            // Проверяем, что предмет существует в сетке
            if (!_itemPositions.ContainsKey(item))
                throw new Exception("Item not found in the grid.");

            // Получаем текущую позицию предмета
            var oldPosition = _itemPositions[item];
            var remainingAmount = amount;
            // Временно удаляем предмет из сетки
            RemoveItem(item);
            var anotherItemAtPosition = GetItemAtPosition(newPosition);
            if (anotherItemAtPosition == null)
            {
                // Проверяем, можно ли разместить предмет на новой позиции
                if (CanPlaceItem(item, newPosition, item.IsRotated.Value))
                {
                    PlaceItem(item, newPosition, item.IsRotated.Value);
                    return new AddItemsToInventoryGridResult(item.ItemType,
                        amount,
                        remainingAmount, true); // Перемещение успешно
                }
                else
                {
                    // Если перемещение невозможно, возвращаем предмет на старую позицию
                    PlaceItem(item, oldPosition, item.IsRotated.Value);
                    return new AddItemsToInventoryGridResult(item.ItemType,
                        amount,
                        0, false); // Перемещение не удалось
                }
            }
            else
            {
                // Если на этой позиции есть предмет то пробуем добавить к нему перемещаемый предмет
                var itemsAddedToSameItems = AddToSameItem(item, anotherItemAtPosition, out remainingAmount);

                if (remainingAmount == 0)
                {
                    // Если предмет поместился без остатка
                    return new AddItemsToInventoryGridResult(item.ItemType,
                        amount, itemsAddedToSameItems, true);
                }
                else
                {
                    // Если все предметы не поместились, возвращаем предмет на старую позицию с остатком который не удалось переместить
                    PlaceItem(item, oldPosition, item.IsRotated.Value);
                    return new AddItemsToInventoryGridResult(item.ItemType,
                        amount, itemsAddedToSameItems, false); // Переместить все предметы не удалось
                }
            }
        }

        public void SortItems(Func<ItemDataProxy, object> sortBy)
        {
            // Временно удаляем все предметы из сетки
            var items = _itemPositions.Select(kvp => kvp.Key).ToList();

            foreach (var item in items)
            {
                RemoveItem(item);
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

        public bool Has(ItemDataProxy item, int amount)
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

            _itemPositions[item] = position;
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

            foreach (var kvp in _itemPositions)
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


        // Инициализация InventoryGridDataProxy в InventoryGridViewModel 

        private void InitializeGrid(InventoryGridDataProxy gridDataProxy)
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