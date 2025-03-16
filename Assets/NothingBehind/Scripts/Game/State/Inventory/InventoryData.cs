using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    [Serializable]
    public class InventoryData
    {
        public int OwnerId;
        public EntityType OwnerType;
        public List<InventoryGridData> InventoryGrids;
    }
}