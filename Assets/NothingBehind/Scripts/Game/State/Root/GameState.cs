using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities.Hero;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventory;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.State.Root
{
    [Serializable]
    public class GameState
    {
        public int GlobalEntityId;
        public int GlobalItemId;
        public MapId CurrentMapId;
        public PlayerData playerData;
        public List<MapData> Maps;
        public List<ResourceData> Resources;
        public List<InventoryData> Inventories;

        public int CreateEntityId()
        {
            return GlobalEntityId++;
        }
        
        public int CreateItemId()
        {
            return GlobalItemId++;
        }
    }
}