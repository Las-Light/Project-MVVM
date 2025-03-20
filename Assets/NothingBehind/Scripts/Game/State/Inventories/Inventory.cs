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
            inventoryData.InventoryGrids.ForEach(gridData =>
            {
                if (gridData is InventoryGridWithSubGridData subGridData)
                {
                    Debug.Log(subGridData.GridId);
                    InventoryGrids.Add(new InventoryGridWithSubGrid(subGridData));
                }
                else
                {
                    InventoryGrids.Add(new InventoryGrid(gridData));
                }
            });

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
                var removedGridProxy = e.Value;
                var removedGrid =
                    inventoryData.InventoryGrids.FirstOrDefault(data => data.GridType == removedGridProxy.GridType);
                inventoryData.InventoryGrids.Remove(removedGrid);
            });
        }
    }
}