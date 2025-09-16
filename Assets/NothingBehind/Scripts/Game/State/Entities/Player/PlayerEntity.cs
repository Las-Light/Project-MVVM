using System.Linq;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Inventories;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Weapons;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Entities.Player
{
    public class PlayerEntity: Entity
    {
        public ReactiveProperty<Inventory> Inventory { get; }
        public ReactiveProperty<Equipment> Equipment { get; }
        public ReactiveProperty<Arsenal> Arsenal { get; }
        public ReactiveProperty<float> Health { get; }
        public ReactiveProperty<MapId> CurrentMapId { get; }
        public ObservableList<PositionOnMap> PositionOnMaps { get; } = new();

        public PlayerEntity(PlayerEntityData entityData) : base(entityData)
        {
            InitPosOnMaps(entityData);

            CurrentMapId = new ReactiveProperty<MapId>(entityData.CurrentMapId);
            Inventory = new ReactiveProperty<Inventory>(new Inventory(entityData.InventoryData));
            Equipment = new ReactiveProperty<Equipment>(new Equipment(entityData.EquipmentData));
            Arsenal = new ReactiveProperty<Arsenal>(new Arsenal(entityData.ArsenalData));
            Health = new ReactiveProperty<float>(entityData.Health);
            CurrentMapId.Skip(1).Subscribe(value => entityData.CurrentMapId = value);
            Inventory.Skip(1).Subscribe(inventory => entityData.InventoryData = inventory.Origin);
            Equipment.Skip(1).Subscribe(equipment => entityData.EquipmentData = equipment.Origin);
            Arsenal.Skip(1).Subscribe(arsenal => entityData.ArsenalData = arsenal.Origin);
            Health.Skip(1).Subscribe(value => entityData.Health = value);
        }

        private void InitPosOnMaps(PlayerEntityData playerEntityData)
        {
            playerEntityData.PositionOnMaps.ForEach(positionOnMap => PositionOnMaps.Add(new PositionOnMap(positionOnMap)));
            
            PositionOnMaps.ObserveAdd().Subscribe(e =>
            {
                var addedPosOnMap = e.Value;
                playerEntityData.PositionOnMaps.Add(addedPosOnMap.Origin);
            });
            PositionOnMaps.ObserveRemove().Subscribe(e =>
            {
                var removedPosOnMapProxy = e.Value;
                var removedPosOnMap =
                    playerEntityData.PositionOnMaps.FirstOrDefault(c => c.MapId == removedPosOnMapProxy.MapId);
                playerEntityData.PositionOnMaps.Remove(removedPosOnMap);
            });
        }
    }
}