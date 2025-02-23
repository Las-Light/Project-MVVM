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
    public class InventoryViewModel
    {
        public readonly int OwnerId;
        public readonly string OwnerTypeId;

        private readonly ICommandProcessor _commandProcessor;
        private readonly InventoryService _inventoryService;

        private readonly ObservableList<InventoryGridViewModel> _allInventoryGrids = new();
        private readonly Dictionary<string, InventoryGridViewModel> _inventoryGridMap = new();
        private readonly Dictionary<string, InventoryGridSettings> _inventoryGridSettingsMap = new();

        public IObservableCollection<InventoryGridViewModel> AllInventoryGrids => _allInventoryGrids;

        public InventoryViewModel(InventoryDataProxy inventoryDataProxy,
            InventorySettings inventorySettings,
            ICommandProcessor commandProcessor,
            InventoryService inventoryService)
        {
            OwnerId = inventoryDataProxy.OwnerId;
            OwnerTypeId = inventoryDataProxy.OwnerTypeId;
            _commandProcessor = commandProcessor;
            _inventoryService = inventoryService;

            foreach (var inventoryGrid in inventorySettings.InventoryGrids)
            {
                _inventoryGridSettingsMap[inventoryGrid.GridTypeId] = inventoryGrid;
            }

            foreach (var inventoryGridDataProxy in inventoryDataProxy.Inventories)
            {
                CreateInventoryGridViewModel(inventoryGridDataProxy);
            }

            inventoryDataProxy.Inventories.ObserveAdd().Subscribe(e => { CreateInventoryGridViewModel(e.Value); });
            inventoryDataProxy.Inventories.ObserveRemove().Subscribe(e => { RemoveInventoryGridViewModel(e.Value); });
        }

        public bool CreateGridInventory(string gridTypeId)
        {
            var command = new CmdCreateGridInventory(OwnerTypeId, OwnerId, gridTypeId);
            var result = _commandProcessor.Process(command);

            return result;
        }

        public bool RemoveGridInventory(string gridTypeId)
        {
            var command = new CmdRemoveGridInventory(OwnerId, gridTypeId);
            var result = _commandProcessor.Process(command);

            return result;
        }

        public AddItemsToInventoryGridResult TryMoveItemInGrid(string gridTypeId, ItemDataProxy item, Vector2Int position, int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel))
            {
                return gridViewModel.TryMoveItem(item, position, amount);
            }

            throw new Exception($"Grid {gridTypeId} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        public AddItemsToInventoryGridResult TryMoveItemToAnotherGrid(string gridTypeIdAt, string gridTypeIdTo, 
            ItemDataProxy item, Vector2Int position, int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeIdAt, out var gridViewModelAt))
            {
                var oldPosition = gridViewModelAt.GetItemPosition(item);
                if (oldPosition != null)
                {
                    gridViewModelAt.RemoveItem(item);
                    if (_inventoryGridMap.TryGetValue(gridTypeIdTo, out var gridViewModelTo))
                    {
                        var addedResult = gridViewModelTo.AddItems(item, position, amount);
                        if (addedResult.Success)
                        {
                            return addedResult;
                        }
                        else
                        {
                            gridViewModelAt.AddItems(item, oldPosition.Value, addedResult.ItemsNotAddedAmount);
                            return new AddItemsToInventoryGridResult(item.Id, amount, addedResult.ItemsAddedAmount,
                                false);
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
                    throw new Exception($"Item {item.Id} in {gridTypeIdAt} don't have " +
                                        $"position in the inventory owner {OwnerTypeId} - {OwnerId}.");
                }
            }
            throw new Exception($"Grid {gridTypeIdAt} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        public AddItemsToInventoryGridResult TryMoveItemToAnotherGrid(string gridTypeIdAt, string gridTypeIdTo, ItemDataProxy item, int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeIdAt, out var gridViewModelAt))
            {
                var oldPosition = gridViewModelAt.GetItemPosition(item);
                if (oldPosition != null)
                {
                    gridViewModelAt.RemoveItem(item);
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
                            return gridViewModelAt.AddItems(item, oldPosition.Value, addedResult.ItemsNotAddedAmount);
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
                    throw new Exception($"Item {item.Id} in {gridTypeIdAt} don't have " +
                                        $"position in the inventory owner {OwnerTypeId} - {OwnerId}.");
                }
            }
            throw new Exception($"Grid {gridTypeIdAt} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        public AddItemsToInventoryGridResult TryAddToGrid(string gridTypeId, ItemDataProxy item, Vector2Int position,
            int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel))
            {
                return gridViewModel.AddItems(item, position, amount);
            }
            throw new Exception($"Grid {gridTypeId} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }

        public RemoveItemsFromInventoryGridResult TryRemoveItem(string gridTypeId, ItemDataProxy item)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel))
            {
                return gridViewModel.RemoveItem(item);
            }
            throw new Exception($"Grid {gridTypeId} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        }
        
        public AddItemsToInventoryGridResult TryAddToGrid(string gridTypeId, ItemDataProxy item, int amount)
        {
            if (_inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel))
            {
                return gridViewModel.AddItems(item, amount);
            }
            throw new Exception($"Grid {gridTypeId} not found " +
                                $"in the inventory owner {OwnerTypeId} - {OwnerId}.");
        } 
            
        [CanBeNull] public InventoryGridViewModel GetInventoryGridViewModel(string gridTypeId)
        {
            return _inventoryGridMap.TryGetValue(gridTypeId, out var gridViewModel) ? gridViewModel : null;
        } 
        private void RemoveInventoryGridViewModel(InventoryGridDataProxy inventoryGridDataProxy)
        {
            if (_inventoryGridMap.TryGetValue(inventoryGridDataProxy.GridTypeId, out var gridViewModel))
            {
                _allInventoryGrids.Remove(gridViewModel);
                _inventoryGridMap.Remove(inventoryGridDataProxy.GridTypeId);
            }
        }

        private void CreateInventoryGridViewModel(InventoryGridDataProxy inventoryGridDataProxy)
        {
            var gridSettings = _inventoryGridSettingsMap[inventoryGridDataProxy.GridTypeId];
            var gridViewModel = new InventoryGridViewModel(inventoryGridDataProxy,
                gridSettings);

            _allInventoryGrids.Add(gridViewModel);
            _inventoryGridMap[inventoryGridDataProxy.GridTypeId] = gridViewModel;
        }
    }
}