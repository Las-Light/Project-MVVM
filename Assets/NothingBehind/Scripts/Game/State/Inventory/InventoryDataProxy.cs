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
        public ObservableList<InventoryGridDataProxy> Inventories { get; } = new();

        public InventoryDataProxy(InventoryData inventoryData)
        {
            Origin = inventoryData;
            OwnerId = inventoryData.OwnerId;
            OwnerTypeId = inventoryData.OwnerTypeId;
            inventoryData.Inventories.ForEach(gridData => Inventories.Add(new InventoryGridDataProxy(gridData)));

            Inventories.ObserveAdd().Subscribe(e =>
            {
                var addedInventorGridProxy = e.Value;
                inventoryData.Inventories.Add(addedInventorGridProxy.Origin);
            });
            Inventories.ObserveRemove().Subscribe(e =>
            {
                var removedGridProxy = e.Value;
                var removedGrid =
                    inventoryData.Inventories.FirstOrDefault(data => data.GridTypeId == removedGridProxy.GridTypeId);
                inventoryData.Inventories.Remove(removedGrid);
            });
        }
    }
}