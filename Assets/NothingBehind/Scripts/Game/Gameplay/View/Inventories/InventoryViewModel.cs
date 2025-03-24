using System;
using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.InventoriesCommands;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class InventoryViewModel : IDisposable
    {
        public readonly int OwnerId;
        public readonly EntityType OwnerType;

        public IObservableCollection<InventoryGridViewModel> AllInventoryGrids => _allInventoryGrids;

        private readonly ICommandProcessor _commandProcessor;
        private readonly InventoryService _inventoryService;

        private readonly ObservableList<InventoryGridViewModel> _allInventoryGrids = new();
        private readonly Dictionary<int, InventoryGridViewModel> _inventoryGridMap = new();
        private readonly Dictionary<InventoryGridType, InventoryGridSettings> _inventoryGridSettingsMap = new();
        private readonly Dictionary<int, Item> _allInventoryItems = new();

        private IReadOnlyObservableDictionary<SlotType, Item> EquipmentItems { get; }

        private readonly CompositeDisposable _disposables = new();


        public InventoryViewModel(Inventory inventory,
            EquipmentService equipmentService,
            InventorySettings inventorySettings,
            ICommandProcessor commandProcessor,
            InventoryService inventoryService)
        {
            OwnerId = inventory.OwnerId;
            OwnerType = inventory.OwnerType;
            _commandProcessor = commandProcessor;
            _inventoryService = inventoryService;

            foreach (var inventoryGrid in inventorySettings.GridsSettings)
            {
                if (inventoryGrid.SubGrids.Count > 0)
                {
                    foreach (var subGrid in inventoryGrid.SubGrids)
                    {
                        _inventoryGridSettingsMap[subGrid.GridType] = subGrid;
                    }
                }

                _inventoryGridSettingsMap[inventoryGrid.GridType] = inventoryGrid;
            }

            if (equipmentService.EquipmentViewModelsMap.TryGetValue(OwnerId, out var equipmentViewModel))
            {
                EquipmentItems = equipmentViewModel.AllEquippedItems;
                foreach (var equippedItem in EquipmentItems)
                {
                    if (equippedItem.Key is SlotType.Backpack or SlotType.ChestRig)
                    {
                        if (equippedItem.Value is GridItem gridItem)
                        {
                            var inventoryGrid =
                                inventory.InventoryGrids.FirstOrDefault(grid => grid.GridId == gridItem.GridId);
                            if (inventoryGrid == null)
                            {
                                AddGridToInventory(OwnerId, gridItem.Grid.Value);
                            }
                        }
                    }
                }

                _disposables.Add(EquipmentItems.ObserveRemove().Subscribe(e =>
                {
                    var removedItem = e.Value.Value;
                    if (removedItem is GridItem removedGridItem)
                    {
                        if (removedGridItem.Grid.Value is InventoryGridWithSubGrid subGrid)
                        {
                            foreach (var grid in subGrid.SubGrids)
                            {
                                RemoveInventoryGridViewModel(grid);
                            }
                        }
                        RemoveGridFromInventory(OwnerId, removedGridItem.Grid.Value);
                    }
                }));

                _disposables.Add(EquipmentItems.ObserveAdd().Subscribe(e =>
                {
                var addedItem = e.Value.Value;
                    if (addedItem is GridItem addedGridItem)
                    {
                        var inventoryGrid =
                            inventory.InventoryGrids.FirstOrDefault(grid => grid.GridId == addedGridItem.GridId);
                        if (inventoryGrid == null)
                        {
                            AddGridToInventory(OwnerId, addedGridItem.Grid.Value);
                        }
                    }
                }));
            }

            foreach (var inventoryGrid in inventory.InventoryGrids)
            {
                if (inventoryGrid is InventoryGridWithSubGrid gridWithSubGrid)
                {
                    foreach (var subGrid in gridWithSubGrid.SubGrids)
                    {
                        CreateInventoryGridViewModel(subGrid);
                    }
                }
                else
                {
                    CreateInventoryGridViewModel(inventoryGrid);
                }
            }

            _disposables.Add(inventory.InventoryGrids.ObserveAdd()
                .Subscribe(e =>
                {
                    var addedGrid = e.Value;
                    if (addedGrid is InventoryGridWithSubGrid gridWithSubGrid)
                        foreach (var subGrid in gridWithSubGrid.SubGrids)
                        {
                            CreateInventoryGridViewModel(subGrid);
                        }
                    else
                    {
                        CreateInventoryGridViewModel(addedGrid);
                    }
                }));
            _disposables.Add(inventory.InventoryGrids.ObserveRemove()
                .Subscribe(e => RemoveInventoryGridViewModel(e.Value)));
        }

        // Создает InventorGridDataProxy
        public bool AddGridToInventory(int ownerId, InventoryGrid grid)
        {
            var command = new CmdAddGridToInventory(ownerId, grid);
            var result = _commandProcessor.Process(command);

            return result;
        }

        // Удаляет InventorGridDataProxy
        public bool RemoveGridFromInventory(int ownerId, InventoryGrid grid)
        {
            var command = new CmdRemoveGridFromInventory(ownerId, grid);
            var result = _commandProcessor.Process(command);

            return result;
        }

        private void CreateInventoryGridViewModel(InventoryGrid inventoryGrid)
        {
            var gridSettings = _inventoryGridSettingsMap[inventoryGrid.GridType];
            var gridViewModel = new InventoryGridViewModel(inventoryGrid,
                gridSettings);

            _allInventoryGrids.Add(gridViewModel);
            _inventoryGridMap[inventoryGrid.GridId] = gridViewModel;
        }

        private void RemoveInventoryGridViewModel(InventoryGrid inventoryGridDataProxy)
        {
            if (_inventoryGridMap.TryGetValue(inventoryGridDataProxy.GridId, out var gridViewModel))
            {
                _allInventoryGrids.Remove(gridViewModel);
                _inventoryGridMap.Remove(inventoryGridDataProxy.GridId);
                gridViewModel.Dispose();
            }
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}