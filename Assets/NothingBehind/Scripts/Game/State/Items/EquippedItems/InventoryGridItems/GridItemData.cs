using System;
using NothingBehind.Scripts.Game.State.Inventories.Grids;

namespace NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems
{
    [Serializable]
    public class GridItemData : ItemData
    {
        public int GridId;
        public InventoryGridType GridType;
        public InventoryGridData GridData;
    }
}