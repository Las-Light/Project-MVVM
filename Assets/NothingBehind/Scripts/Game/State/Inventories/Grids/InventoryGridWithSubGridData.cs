using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Inventories.Grids
{
    public class InventoryGridWithSubGridData : InventoryGridData
    {
        public readonly List<InventoryGridData> SubGrids;

        public InventoryGridWithSubGridData(
            int gridId,
            InventoryGridType gridType,
            int width,
            int height, 
            float cellSize,
            bool isSubGrid,
            List<InventoryGridData> subGrids) : base(gridId, gridType, width, height, cellSize, isSubGrid)
        {
            SubGrids = subGrids;
        }
    }
}