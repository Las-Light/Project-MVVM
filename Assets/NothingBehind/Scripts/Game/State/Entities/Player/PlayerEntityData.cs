using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Weapons;

namespace NothingBehind.Scripts.Game.State.Entities.Player
{
    public class PlayerEntityData : EntityData
    {
        public MapId CurrentMapId { get; set; }
        public List<PositionOnMapData> PositionOnMaps { get; set; }
        public InventoryData InventoryData { get; set; }
        public EquipmentData EquipmentData { get; set; }
        public ArsenalData ArsenalData { get; set; }
        public float Health { get; set; }
    }
}