using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Weapons;
using R3;

namespace NothingBehind.Scripts.Game.State.Entities.Characters
{
    public class CharacterEntity: Entity
    {
        public ReactiveProperty<Inventory> Inventory { get; }
        public ReactiveProperty<Equipment> Equipment { get; }
        public ReactiveProperty<Arsenal> Arsenal { get; }
        public ReactiveProperty<float> Health { get; }

        public CharacterEntity(CharacterEntityData entityData) : base(entityData)
        {
            Inventory = new ReactiveProperty<Inventory>(new Inventory(entityData.InventoryData));
            Equipment = new ReactiveProperty<Equipment>(new Equipment(entityData.EquipmentData));
            Arsenal = new ReactiveProperty<Arsenal>(new Arsenal(entityData.ArsenalData));
            Health = new ReactiveProperty<float>(entityData.Health);

            Inventory.Skip(1).Subscribe(inventory => entityData.InventoryData = inventory.Origin);
            Equipment.Skip(1).Subscribe(equipment => entityData.EquipmentData = equipment.Origin);
            Arsenal.Skip(1).Subscribe(arsenal => entityData.ArsenalData = arsenal.Origin);
            Health.Skip(1).Subscribe(value => entityData.Health = value);
        }
    }
}