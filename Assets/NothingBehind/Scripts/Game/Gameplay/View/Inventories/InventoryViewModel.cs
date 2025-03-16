using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NothingBehind.Scripts.Game.Gameplay.Commands.Inventories;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class InventoryViewModel : IDisposable
    {
        public readonly int OwnerId;
        public readonly string OwnerTypeId;

        public IObservableCollection<InventoryGridViewModel> AllInventoryGrids => _allInventoryGrids;

        private readonly ICommandProcessor _commandProcessor;
        private readonly InventoryService _inventoryService;

        private readonly ObservableList<InventoryGridViewModel> _allInventoryGrids = new();
        private readonly Dictionary<string, InventoryGridViewModel> _inventoryGridMap = new();
        private readonly Dictionary<string, InventoryGridSettings> _inventoryGridSettingsMap = new();

        private readonly CompositeDisposable _disposables = new();


        public InventoryViewModel(Inventory inventory,
            InventorySettings inventorySettings,
            ICommandProcessor commandProcessor,
            InventoryService inventoryService)
        {
            OwnerId = inventory.OwnerId;
            OwnerTypeId = inventory.OwnerTypeId;
            _commandProcessor = commandProcessor;
            _inventoryService = inventoryService;

            foreach (var inventoryGrid in inventorySettings.InventoryGrids)
            {
                if (inventoryGrid.SubGrids.Count > 0)
                {
                    foreach (var subGrid in inventoryGrid.SubGrids)
                    {
                        _inventoryGridSettingsMap[subGrid.GridTypeId] = subGrid;
                    }
                }
                _inventoryGridSettingsMap[inventoryGrid.GridTypeId] = inventoryGrid;
            }

            foreach (var inventoryGrid in inventory.InventoryGrids)
            {
                if (inventoryGrid.SubGrids.Count > 0)
                {
                    foreach (var subGrid in inventoryGrid.SubGrids)
                    {
                        CreateInventoryGridViewModel(subGrid);
                    }
                }
                CreateInventoryGridViewModel(inventoryGrid);
            }

            _disposables.Add(inventory.InventoryGrids.ObserveAdd()
                .Subscribe(e => CreateInventoryGridViewModel(e.Value)));
            _disposables.Add(inventory.InventoryGrids.ObserveRemove()
                .Subscribe(e => RemoveInventoryGridViewModel(e.Value)));
        }

        // Создает InventorGridDataProxy
        public bool CreateGridInventory(string gridTypeId)
        {
            var command = new CmdCreateGridInventory(OwnerTypeId, OwnerId, gridTypeId);
            var result = _commandProcessor.Process(command);

            return result;
        }

        // Удаляет InventorGridDataProxy
        public bool RemoveGridInventory(string gridTypeId)
        {
            var command = new CmdRemoveGridInventory(OwnerId, gridTypeId);
            var result = _commandProcessor.Process(command);

            return result;
        }

        [CanBeNull]
        public InventoryGridViewModel GetInventoryGridViewModel(string gridTypeId)
        {
            return _inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel) ? gridViewModel : null;
        }

        // Перемещение предмета по позиции внутри одной сетки
        public AddItemsToInventoryGridResult TryMoveItemInGrid(string gridTypeId, int itemId,
            Vector2Int position, int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel))
            {
                return gridViewModel.TryMoveItem(itemId, position, amount);
            }

            throw new Exception($"Grid {gridTypeId} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        // Перемещение предмета по позиции в другую сетку внутри одного инвентарая 
        public AddItemsToInventoryGridResult TryMoveItemToAnotherGrid(string gridTypeIdAt, string gridTypeIdTo,
            int itemId, Vector2Int position, int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeIdAt, out var gridViewModelAt))
            {
                var oldPosition = gridViewModelAt.GetItemPosition(itemId);
                if (!gridViewModelAt.ItemsMap.TryGetValue(itemId, out var item))
                {
                    throw new Exception($"Item {itemId} not found in the grid {gridTypeIdAt}.");
                }

                if (oldPosition != null)
                {
                    gridViewModelAt.RemoveItem(itemId);
                    if (_inventoryGridMap.TryGetValue(gridTypeIdTo, out var gridViewModelTo))
                    {
                        var addedResult = gridViewModelTo.AddItems(item, position, amount);
                        if (addedResult.Success)
                        {
                            return addedResult;
                        }
                        else
                        {
                            // возвращаем остаток на старую позицию
                            gridViewModelAt.AddItems(item, oldPosition.Value, addedResult.ItemsNotAddedAmount);
                            return new AddItemsToInventoryGridResult(item.ItemType, itemId, amount,
                                addedResult.ItemsAddedAmount, false, false);
                        }
                    }
                    else
                    {
                        throw new Exception($"Grid {gridTypeIdTo} not found " +
                                            $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
                    }
                }
                else
                {
                    throw new Exception($"Item {itemId} in {gridTypeIdAt} don't have " +
                                        $"position in the inventory owner {OwnerTypeId} - {OwnerId}.");
                }
            }

            throw new Exception($"Grid {gridTypeIdAt} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        // Автоперемещение предмета из одной сетки в ругую
        public AddItemsToInventoryGridResult TryMoveItemToAnotherGrid(string gridTypeIdAt, string gridTypeIdTo,
            int itemId, int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeIdAt, out var gridViewModelAt))
            {
                var oldPosition = gridViewModelAt.GetItemPosition(itemId);
                if (!gridViewModelAt.ItemsMap.TryGetValue(itemId, out var item))
                {
                    throw new Exception($"Item {itemId} not found in the grid {gridTypeIdAt}.");
                }

                if (oldPosition != null)
                {
                    gridViewModelAt.RemoveItem(itemId);
                    if (_inventoryGridMap.TryGetValue(gridTypeIdTo, out var gridViewModelTo))
                    {
                        var addedResult = gridViewModelTo.AddItems(item, amount);
                        if (addedResult.Success)
                        {
                            return addedResult;
                        }
                        else
                        {
                            gridViewModelAt.AddItems(item, oldPosition.Value, addedResult.ItemsNotAddedAmount);
                            return new AddItemsToInventoryGridResult(item.ItemType, itemId, amount,
                                addedResult.ItemsAddedAmount, false, false);
                        }
                    }
                    else
                    {
                        throw new Exception($"Grid {gridTypeIdTo} not found " +
                                            $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
                    }
                }
                else
                {
                    throw new Exception($"Item {itemId} in {gridTypeIdAt} don't have " +
                                        $"position in the inventory owner {OwnerTypeId} - {OwnerId}.");
                }
            }

            throw new Exception($"Grid {gridTypeIdAt} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        // Перемещение предмета по позиции из одного инвентаря в другой
        public AddItemsToInventoryGridResult TryMoveItemToAnotherInventory(int ownerIdAt, int ownerIdTo,
            string gridTypeIdAt, string gridTypeIdTo, int itemId, Vector2Int position, int amount)
        {
            return _inventoryService.TryMoveToAnotherInventory(ownerIdAt, ownerIdTo, itemId,
                gridTypeIdAt, gridTypeIdTo, position, amount);
        }

        // Автоперемещение предмета из одного инвентаря в другой
        public AddItemsToInventoryGridResult TryMoveItemToAnotherInventory(int ownerIdAt, int ownerIdTo,
            string gridTypeIdAt, string gridTypeIdTo, int itemId, int amount)
        {
            return _inventoryService.TryMoveToAnotherInventory(ownerIdAt, ownerIdTo, itemId,
                gridTypeIdAt, gridTypeIdTo, amount);
        }

        // Попытаться удалить предмет из сетки
        public RemoveItemsFromInventoryGridResult TryRemoveItem(string gridTypeId, int itemId)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel))
            {
                return gridViewModel.RemoveItem(itemId);
            }

            throw new Exception($"Grid {gridTypeId} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        // Полпытаться удалить некоторое количество из стека предмета
        public RemoveItemsFromInventoryGridResult TryRemoveItem(string gridTypeId, int itemId, int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel))
            {
                return gridViewModel.RemoveItemAmount(itemId, amount);
            }

            throw new Exception($"Grid {gridTypeId} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        public AddItemsToInventoryGridResult TryAddToGrid(string gridTypeId, Item item, Vector2Int position,
            int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel))
            {
                return gridViewModel.AddItems(item, position, amount);
            }

            throw new Exception($"Grid {gridTypeId} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        public AddItemsToInventoryGridResult TryAddToGrid(string gridTypeId, Item item, int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel))
            {
                return gridViewModel.AddItems(item, amount);
            }

            throw new Exception($"Grid {gridTypeId} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        private void CreateInventoryGridViewModel(InventoryGrid inventoryGridDataProxy)
        {
            var gridSettings = _inventoryGridSettingsMap[inventoryGridDataProxy.GridTypeId];
            var gridViewModel = new InventoryGridViewModel(inventoryGridDataProxy,
                gridSettings);

            _allInventoryGrids.Add(gridViewModel);
            _inventoryGridMap[inventoryGridDataProxy.GridTypeId] = gridViewModel;
        }

        private void RemoveInventoryGridViewModel(InventoryGrid inventoryGridDataProxy)
        {
            if (_inventoryGridMap.TryGetValue(inventoryGridDataProxy.GridTypeId, out var gridViewModel))
            {
                _allInventoryGrids.Remove(gridViewModel);
                _inventoryGridMap.Remove(inventoryGridDataProxy.GridTypeId);
                gridViewModel.Dispose();
            }
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}