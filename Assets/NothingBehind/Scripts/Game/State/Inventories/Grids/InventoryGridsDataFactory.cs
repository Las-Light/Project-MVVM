using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.State.Root;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Inventories.Grids
{
    public static class InventoryGridsDataFactory
    {
        public static InventoryGridData CreateInventorGridData(GameState gameState, InventoryGridSettings gridSettings)
        {
            switch (gridSettings.GridType)
            {
                case InventoryGridType.Backpack:
                    var gridData = new InventoryGridData(gameState.CreateGridId(),
                        gridSettings.GridType,
                        gridSettings.Width,
                        gridSettings.Height,
                        gridSettings.CellSize,
                        gridSettings.IsSubGrid);
                    return gridData;

                case InventoryGridType.ChestRig:
                    var subGridsData = new List<InventoryGridData>();
                    foreach (var inventoryGridSettings in gridSettings.SubGrids)
                    {
                        var subGridData = new InventoryGridData(
                            gameState.CreateGridId(),
                            inventoryGridSettings.GridType,
                            inventoryGridSettings.Width,
                            inventoryGridSettings.Height,
                            inventoryGridSettings.CellSize,
                            inventoryGridSettings.IsSubGrid
                            );
                        subGridsData.Add(subGridData);
                    }
                    var parentsGridData = new InventoryGridWithSubGridData(
                        gameState.CreateGridId(),
                        gridSettings.GridType,
                        gridSettings.Width,
                        gridSettings.Height,
                        gridSettings.CellSize,
                        gridSettings.IsSubGrid,
                        subGridsData
                        );
                    return parentsGridData;

                default:
                    throw new Exception("Unsupported grid type: " + gridSettings.GridType);
            }
        }
    }
}