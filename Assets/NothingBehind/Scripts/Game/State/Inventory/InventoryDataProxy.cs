using System.Linq;
using R3;
using ObservableCollections;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    public class InventoryDataProxy
    {
        public int OwnerId { get; }
        public string OwnerTypeId { get; }
        public InventoryData Origin { get; }
        public ObservableList<InventoryGridDataProxy> InventoryGrids { get; } = new();

        public InventoryDataProxy(InventoryData inventoryData)
        {
            Origin = inventoryData;
            OwnerId = inventoryData.OwnerId;
            OwnerTypeId = inventoryData.OwnerTypeId;
            inventoryData.InventoryGrids.ForEach(gridData => InventoryGrids.Add(new InventoryGridDataProxy(gridData)));
            
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