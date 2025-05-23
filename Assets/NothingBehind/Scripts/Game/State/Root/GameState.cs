using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.GlobalMap;
using NothingBehind.Scripts.Game.State.Weapons;

namespace NothingBehind.Scripts.Game.State.Root
{
    [Serializable]
    public class GameState
    {
        public int GlobalEntityId;
        public int GlobalItemId;
        public int GlobalGridId;
        public MapId CurrentMapId;
        public PlayerData PlayerData;
        public GlobalMapData GlobalMap;
        public List<GameplayMapData> GameplayMaps;
        public List<ResourceData> Resources;
        public List<InventoryData> Inventories;
        public List<EquipmentData> Equipments;
        public List<ArsenalData> Arsenals;

        public int CreateEntityId()
        {
            return GlobalEntityId++;
        }
        
        public int CreateItemId()
        {
            return GlobalItemId++;
        }
        
        public int CreateGridId()
        {
            return GlobalGridId++;
        }
    }
}