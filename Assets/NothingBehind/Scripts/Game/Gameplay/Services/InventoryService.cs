using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands.Inventories;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class InventoryService
    {
        public int PlayerId { get; }
        private readonly ICommandProcessor _commandProcessor;

        private readonly ObservableList<InventoryViewModel> _allInventories = new();
        private readonly Dictionary<int, InventoryViewModel> _inventoryMap = new();
        private readonly Dictionary<int, Inventory> _inventoryDataMap = new();
        private readonly Dictionary<EntityType, InventorySettings> _inventorySettingsMap = new();

        public IObservableCollection<InventoryViewModel> AllInventories => _allInventories;

        public Dictionary<int, InventoryViewModel> InventoryMap => _inventoryMap;

        public InventoryService(IObservableCollection<Inventory> inventories,
            InventoriesSettings inventoriesSettings,
            ICommandProcessor commandProcessor, int playerId)
        {
            PlayerId = playerId;
            _commandProcessor = commandProcessor;

            foreach (var inventorySettings in inventoriesSettings.Inventories)
            {
                _inventorySettingsMap[inventorySettings.OwnerType] = inventorySettings;
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

        public bool CreateInventory(EntityType ownerType, int ownerId)
        {
            var command = new CmdCreateInventory(ownerType, ownerId);
            var result = _commandProcessor.Process(command);
            
            return result;
        }

        public bool RemoveInventory(int ownerId)
        {
            var command = new CmdRemoveInventory(ownerId);
            var result = _commandProcessor.Process(command);
            
            return result;
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
                                                $"in the inventory owner {inventoryViewModelTo.OwnerType} " +
                                                $"{inventoryOwnerIdTo}.");
                        }
                        throw new Exception($"Inventory Id " +
                                            $"{inventoryOwnerIdTo} not found in inventoryMap.");
                    }
                    throw new Exception($"Item {itemId} in {gridTypeIdAt} don't have " +
                                        $"position in the inventory owner " +
                                        $"{inventoryViewModelAt.OwnerType} - {inventoryViewModelAt.OwnerId}.");
                }
                else
                {
                    throw new Exception($"Grid {gridTypeIdAt} not found " +
                                        $"in the inventory owner {inventoryViewModelAt.OwnerType} " +
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
                                                $"in the inventory owner {inventoryViewModelTo.OwnerType} " +
                                                $"{inventoryOwnerIdTo}.");
                        }
                        throw new Exception($"Inventory Id " +
                                            $"{inventoryOwnerIdTo} not found in inventoryMap.");
                    }
                    throw new Exception($"Item {itemId} in {gridTypeIdAt} don't have " +
                                        $"position in the inventory owner " +
                                        $"{inventoryViewModelAt.OwnerType} - {inventoryViewModelAt.OwnerId}.");
                }
                else
                {
                    throw new Exception($"Grid {gridTypeIdAt} not found " +
                                        $"in the inventory owner {inventoryViewModelAt.OwnerType} " +
                                        $"{inventoryOwnerIdAt}.");
                }
            }
            else
            {
                throw new Exception($"Inventory Id " +
                                    $"{inventoryOwnerIdAt} not found in inventoryMap.");
            }
        }

        public InventoryViewModel CreateInventoryViewModel(int ownerId)
        {
            if (_inventoryDataMap.TryGetValue(ownerId, out var inventoryDataProxy))
            {
                var inventorySettings = _inventorySettingsMap[inventoryDataProxy.OwnerType];
                var inventoryViewModel = new InventoryViewModel(inventoryDataProxy,
                    inventorySettings,
                    _commandProcessor,
                    this);

                _allInventories.Add(inventoryViewModel);
                _inventoryMap[inventoryDataProxy.OwnerId] = inventoryViewModel;
                return inventoryViewModel;
            }

            return null;
        }

        public void RemoveInventoryViewModel(Inventory inventory)
        {
            if (_inventoryMap.TryGetValue(inventory.OwnerId, out var inventoryViewModel))
            {
                _allInventories.Remove(inventoryViewModel);
                _inventoryMap.Remove(inventory.OwnerId);
                inventoryViewModel.Dispose();
            }
        }
    }
}