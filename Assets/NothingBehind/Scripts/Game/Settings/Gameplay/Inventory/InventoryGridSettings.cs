using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Grid Config", menuName = "Inventory/Grid Config", order = 2)]
    public class InventoryGridSettings : ScriptableObject
    {
        public InventoryGridType GridType;
        public int Width;
        public int Height;
        public float CellSize;
        public bool IsSubGrid;
        public List<InventoryGridSettings> SubGrids;
    }
}