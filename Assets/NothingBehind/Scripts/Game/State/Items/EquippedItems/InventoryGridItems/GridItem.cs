using NothingBehind.Scripts.Game.State.Inventories.Grids;
using R3;

namespace NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems
{
    public class GridItem : Item
    {
        public int GridId { get; }
        public InventoryGridType GridType { get; }
        public ReactiveProperty<InventoryGrid> Grid;

        public GridItem(GridItemData gridItemData) : base(gridItemData)
        {
            GridId = gridItemData.GridId;
            GridType = gridItemData.GridType;
            if (gridItemData.GridData is InventoryGridWithSubGridData subGridData)
            {
                Grid = new ReactiveProperty<InventoryGrid>(new InventoryGridWithSubGrid(subGridData));
            }
            else
            {
                Grid = new ReactiveProperty<InventoryGrid>(new InventoryGrid(gridItemData.GridData));
            }
        }
    }
}