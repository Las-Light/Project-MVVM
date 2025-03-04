using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NothingBehind.Scripts.Game.Gameplay.Commands.Inventories;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class InventoryService
    {
        private readonly ICommandProcessor _commandProcessor;

        private readonly ObservableList<InventoryViewModel> _allInventories = new();
        private readonly Dictionary<int, InventoryViewModel> _inventoryMap = new();
        private readonly Dictionary<int, InventoryDataProxy> _inventoryDataMap = new();
        private readonly Dictionary<string, InventorySettings> _inventorySettingsMap = new();

        public IObservableCollection<InventoryViewModel> AllInventories => _allInventories;

        public Dictionary<int, InventoryViewModel> InventoryMap => _inventoryMap;

        public InventoryService(IObservableCollection<InventoryDataProxy> inventories,
            InventoriesSettings inventoriesSettings,
            ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;

            foreach (var inventorySettings in inventoriesSettings.Inventories)
            {
                _inventorySettingsMap[inventorySettings.OwnerTypeId] = inventorySettings;
            }

            foreach (var inventoryDataProxy in inventories)
            {
                _inventoryDataMap[inventoryDataProxy.OwnerId] = inventoryDataProxy;
            }

            inventories.ObserveAdd().Subscribe(e =>
            {
                _inventoryDataMap[e.Value.OwnerId] = e.Value;
            });
            inventories.ObserveRemove().Subscribe(e =>
            {
                _inventoryDataMap.Remove(e.Value.OwnerId);
            });
        }

        public bool CreateInventory(string ownerTypeId, int ownerId)
        {
            var command = new CmdCreateInventory(ownerTypeId, ownerId);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        public bool RemoveInventory(int ownerId)
        {
            var command = new CmdRemoveInventory(ownerId);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        [CanBeNull]
        public InventoryDataProxy GetInventoryDataProxy(int ownerId)
        {
            return _inventoryDataMap.TryGetValue(ownerId, out var inventoryData) ? inventoryData : null;
        }

        public AddItemsToInventoryGridResult TryMoveToAnotherInventory(int inventoryOwnerIdAt, int inventoryOwnerIdTo,
            int itemId, string gridTypeIdAt, string gridTypeIdTo, int amount)
        {
            if (_inventoryMap.TryGetValue(inventoryOwnerIdAt, out var inventoryViewModelAt))
            {
                var gridViewModelAt = inventoryViewModelAt.GetInventoryGridViewModel(gridTypeIdAt);
                if (gridViewModelAt != null)
                {
                    var oldPosition = gridViewModelAt.GetItemPosition(itemId);
                    if (!gridViewModelAt.ItemsMap.TryGetValue(itemId, out var item))
                    {
                        throw new Exception($"Item {itemId} not found in the grid {gridTypeIdAt}.");
                    }
                    
                    if (oldPosition != null)
                    {
                        gridViewModelAt.RemoveItem(itemId);
                        if (_inventoryMap.TryGetValue(inventoryOwnerIdTo, out var inventoryViewModelTo))
                        {
                            var gridViewModelTo = inventoryViewModelTo.GetInventoryGridViewModel(gridTypeIdTo);
                            if (gridViewModelTo != null)
                            {
                                var addedResult = gridViewModelTo.AddItems(item, amount);
                                if (addedResult.Success)
                                {
                                    return addedResult;
                                }
                                else
                                {
                                    gridViewModelAt.AddItems(item, oldPosition.Value,
                                        addedResult.ItemsNotAddedAmount);
                                    return new AddItemsToInventoryGridResult(item.ItemType, itemId, amount, addedResult.ItemsAddedAmount, false, false);
                                }
                            }
                            throw new Exception($"Grid {gridTypeIdTo} not found " +
                                                $"in the inventory owner {inventoryViewModelTo.OwnerTypeId} " +
                                                $"{inventoryOwnerIdTo}.");
                        }
                        throw new Exception($"Inventory Id " +
                                            $"{inventoryOwnerIdTo} not found in inventoryMap.");
                    }
                    throw new Exception($"Item {itemId} in {gridTypeIdAt} don't have " +
                                        $"position in the inventory owner " +
                                        $"{inventoryViewModelAt.OwnerTypeId} - {inventoryViewModelAt.OwnerId}.");
                }
                else
                {
                    throw new Exception($"Grid {gridTypeIdAt} not found " +
                                        $"in the inventory owner {inventoryViewModelAt.OwnerTypeId} " +
                                        $"{inventoryOwnerIdAt}.");
                }
            }
            else
            {
                throw new Exception($"Inventory Id " +
                                    $"{inventoryOwnerIdAt} not found in inventoryMap.");
            }
        }

        public AddItemsToInventoryGridResult TryMoveToAnotherInventory(int inventoryOwnerIdAt, int inventoryOwnerIdTo, 
            int itemId, string gridTypeIdAt, string gridTypeIdTo, Vector2Int position, int amount)
        {
            if (_inventoryMap.TryGetValue(inventoryOwnerIdAt, out var inventoryViewModelAt))
            {
                var gridViewModelAt = inventoryViewModelAt.GetInventoryGridViewModel(gridTypeIdAt);
                if (gridViewModelAt != null)
                {
                    var oldPosition = gridViewModelAt.GetItemPosition(itemId);
                    if (!gridViewModelAt.ItemsMap.TryGetValue(itemId, out var item))
                    {
                        throw new Exception($"Item {itemId} not found in the grid {gridTypeIdAt}.");
                    }
                    
                    if (oldPosition != null)
                    {
                        gridViewModelAt.RemoveItem(itemId);
                        if (_inventoryMap.TryGetValue(inventoryOwnerIdTo, out var inventoryViewModelTo))
                        {
                            var gridViewModelTo = inventoryViewModelTo.GetInventoryGridViewModel(gridTypeIdTo);
                            if (gridViewModelTo != null)
                            {
                                var addedResult = gridViewModelTo.AddItems(item, position, amount);
                                if (addedResult.Success)
                                {
                                    return addedResult;
                                }
                                else
                                {
                                    gridViewModelAt.AddItems(item, oldPosition.Value,
                                        addedResult.ItemsNotAddedAmount);
                                    return new AddItemsToInventoryGridResult(item.ItemType, itemId, amount, addedResult.ItemsAddedAmount, false, false);
                                }
                            }
                            throw new Exception($"Grid {gridTypeIdTo} not found " +
                                                $"in the inventory owner {inventoryViewModelTo.OwnerTypeId} " +
                                                $"{inventoryOwnerIdTo}.");
                        }
                        throw new Exception($"Inventory Id " +
                                            $"{inventoryOwnerIdTo} not found in inventoryMap.");
                    }
                    throw new Exception($"Item {itemId} in {gridTypeIdAt} don't have " +
                                        $"position in the inventory owner " +
                                        $"{inventoryViewModelAt.OwnerTypeId} - {inventoryViewModelAt.OwnerId}.");
                }
                else
                {
                    throw new Exception($"Grid {gridTypeIdAt} not found " +
                                        $"in the inventory owner {inventoryViewModelAt.OwnerTypeId} " +
                                        $"{inventoryOwnerIdAt}.");
                }
            }
            else
            {
                throw new Exception($"Inventory Id " +
                                    $"{inventoryOwnerIdAt} not found in inventoryMap.");
            }
        }

        public InventoryViewModel CreateInventoryViewModel(InventoryDataProxy inventoryDataProxy)
        {
            var inventorySettings = _inventorySettingsMap[inventoryDataProxy.OwnerTypeId];
            var inventoryViewModel = new InventoryViewModel(inventoryDataProxy,
                inventorySettings,
                _commandProcessor,
                this);

            _allInventories.Add(inventoryViewModel);
            _inventoryMap[inventoryDataProxy.OwnerId] = inventoryViewModel;
            return inventoryViewModel;
        }

        public void RemoveInventoryViewModel(InventoryDataProxy inventoryDataProxy)
        {
            if (_inventoryMap.TryGetValue(inventoryDataProxy.OwnerId, out var inventoryViewModel))
            {
                _allInventories.Remove(inventoryViewModel);
                _inventoryMap.Remove(inventoryDataProxy.OwnerId);
            }
        }
    }
}