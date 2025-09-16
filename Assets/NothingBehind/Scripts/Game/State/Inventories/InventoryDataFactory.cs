using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Inventories.Grids;

namespace NothingBehind.Scripts.Game.State.Inventories
{
    public static class InventoryDataFactory
    {
        public static InventoryData CreateInventoryData(EntityType entityType, int ownerId)
        {
            var inventory = new InventoryData()
            {
                OwnerId = ownerId,
                OwnerType = entityType
            };
            var inventoryGrids = new List<InventoryGridData>();

            inventory.InventoryGrids = inventoryGrids;

            return inventory;
        }
    }
}