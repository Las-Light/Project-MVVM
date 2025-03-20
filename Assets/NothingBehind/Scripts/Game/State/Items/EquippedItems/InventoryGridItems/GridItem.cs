using NothingBehind.Scripts.Game.State.Inventories.Grids;
using R3;

namespace NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems
{
    public class GridItem : Item
    {
        public int GridId { get; }
        public InventoryGridType GridType { get; }
        public ReactiveProperty<InventoryGrid> Grid;

        public GridItem(GridItemData itemData) : base(itemData)
        {
            GridId = itemData.GridId;
            GridType = itemData.GridType;
            if (itemData.GridData is InventoryGridWithSubGridData subGridData)
            {
                Grid = new ReactiveProperty<InventoryGrid>(new InventoryGridWithSubGrid(subGridData));
            }
            else
            {
                Grid = new ReactiveProperty<InventoryGrid>(new InventoryGrid(itemData.GridData));
            }
        }
    }
}