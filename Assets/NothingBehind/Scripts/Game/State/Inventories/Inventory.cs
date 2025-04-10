using System.Linq;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Inventories
{
    public class Inventory
    {
        public int OwnerId { get; }
        public EntityType OwnerType { get; }
        public InventoryData Origin { get; }
        public ObservableList<InventoryGrid> InventoryGrids { get; } = new();

        public Inventory(InventoryData inventoryData)
        {
            Origin = inventoryData;
            OwnerId = inventoryData.OwnerId;
            OwnerType = inventoryData.OwnerType;

            if (OwnerType == EntityType.Storage && inventoryData.InventoryGrids.Count > 0)
            {
                inventoryData.InventoryGrids.ForEach(data => InventoryGrids.Add(new InventoryGrid(data)));
            }

            InventoryGrids.ObserveAdd().Subscribe(e =>
            {
                var addedInventorGrid = e.Value;
                if (addedInventorGrid is InventoryGridWithSubGrid subGrid)
                {
                    inventoryData.InventoryGrids.Add(subGrid.Origin);
                }
                else
                {
                    inventoryData.InventoryGrids.Add(addedInventorGrid.Origin);
                }
            });
            InventoryGrids.ObserveRemove().Subscribe(e =>
            {
                var removedGrid = e.Value;
                var removedGridData =
                    inventoryData.InventoryGrids.FirstOrDefault(data => data.GridId == removedGrid.GridId);
                inventoryData.InventoryGrids.Remove(removedGridData);
            });
        }
    }
}