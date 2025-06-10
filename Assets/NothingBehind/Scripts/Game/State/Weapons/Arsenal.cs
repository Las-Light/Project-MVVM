using System.Linq;
using NothingBehind.Scripts.Game.State.Equipments;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class Arsenal
    {
        public ArsenalData Origin { get; }
        public int OwnerId { get; }
        public ReactiveProperty<SlotType> CurrentWeaponSlot { get; }
        public ObservableList<Weapon> Weapons { get; } = new();

        public Arsenal(ArsenalData data)
        {
            Origin = data;
            OwnerId = data.OwnerId;
            CurrentWeaponSlot = new ReactiveProperty<SlotType>(data.CurrentWeaponSlot);
            data.Weapons.ForEach(weaponData => Weapons.Add(new Weapon(weaponData)));

            CurrentWeaponSlot.Skip(1).Subscribe(e => { data.CurrentWeaponSlot = e; });
            Weapons.ObserveAdd().Subscribe(e =>
            {
                var addedWeapon = e.Value;
                data.Weapons.Add(addedWeapon.Origin);
            });
            Weapons.ObserveRemove().Subscribe(e =>
            {
                var removedWeapon = e.Value;
                var removedWeaponData = data.Weapons.FirstOrDefault(weaponData => weaponData.Id == removedWeapon.Id);
                data.Weapons.Remove(removedWeaponData);
            });
        }
    }
}