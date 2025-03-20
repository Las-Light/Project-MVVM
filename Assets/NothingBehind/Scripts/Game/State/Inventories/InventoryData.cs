using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Inventories.Grids;

namespace NothingBehind.Scripts.Game.State.Inventories
{
    [Serializable]
    public class InventoryData
    {
        public int OwnerId;
        public EntityType OwnerType;
        public List<InventoryGridData> InventoryGrids;
    }
}