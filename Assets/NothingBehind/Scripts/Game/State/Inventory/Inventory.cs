using System.Linq;
using R3;
using ObservableCollections;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    public class Inventory
    {
        public int OwnerId { get; }
        public string OwnerTypeId { get; }
        public InventoryData Origin { get; }
        public ObservableList<InventoryGrid> InventoryGrids { get; } = new();

        public Inventory(InventoryData inventoryData)
        {
            Origin = inventoryData;
            OwnerId = inventoryData.OwnerId;
            OwnerTypeId = inventoryData.OwnerTypeId;
            inventoryData.InventoryGrids.ForEach(gridData => InventoryGrids.Add(new InventoryGrid(gridData)));
            
            InventoryGrids.ObserveAdd().Subscribe(e =>
            {
                var addedInventorGridProxy = e.Value;
                inventoryData.InventoryGrids.Add(addedInventorGridProxy.Origin);
            });
            InventoryGrids.ObserveRemove().Subscribe(e =>
            {
                var removedGridProxy = e.Value;
                var removedGrid =
                    inventoryData.InventoryGrids.FirstOrDefault(data => data.GridTypeId == removedGridProxy.GridTypeId);
                inventoryData.InventoryGrids.Remove(removedGrid);
            });
        }
    }
}