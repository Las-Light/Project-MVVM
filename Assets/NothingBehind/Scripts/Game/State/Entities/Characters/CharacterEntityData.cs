using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Weapons;

namespace NothingBehind.Scripts.Game.State.Entities.Characters
{
    public class CharacterEntityData : EntityData
    {
        public InventoryData InventoryData { get; set; }
        public EquipmentData EquipmentData { get; set; }
        public ArsenalData ArsenalData { get; set; }
        public float Health { get; set; }
    }
}