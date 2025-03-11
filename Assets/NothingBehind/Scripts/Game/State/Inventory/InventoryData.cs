using System;
using System.Collections.Generic;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    [Serializable]
    public class InventoryData
    {
        public int OwnerId;
        public string OwnerTypeId;
        public List<InventoryGridData> InventoryGrids;
    }
}