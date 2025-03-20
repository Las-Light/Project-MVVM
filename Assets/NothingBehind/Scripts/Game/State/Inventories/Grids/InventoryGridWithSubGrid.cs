using System.Linq;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Inventories.Grids
{
    public class InventoryGridWithSubGrid : InventoryGrid
    {
        public new readonly InventoryGridWithSubGridData Origin;
        public ObservableList<InventoryGrid> SubGrids { get; } = new();
        
        public InventoryGridWithSubGrid(InventoryGridWithSubGridData inventorGridData) : base(inventorGridData)
        {
            Origin = inventorGridData;
            inventorGridData.SubGrids.ForEach(gridData => SubGrids.Add(new InventoryGrid(gridData)));

            SubGrids.ObserveAdd().Subscribe(e =>
            {
                var addedInventorGridProxy = e.Value;
                inventorGridData.SubGrids.Add(addedInventorGridProxy.Origin);
            });
            SubGrids.ObserveRemove().Subscribe(e =>
            {
                var removedGridProxy = e.Value;
                var removedGrid =
                    inventorGridData.SubGrids.FirstOrDefault(data => data.GridType == removedGridProxy.GridType);
                inventorGridData.SubGrids.Remove(removedGrid);
            });
        }
    }
}