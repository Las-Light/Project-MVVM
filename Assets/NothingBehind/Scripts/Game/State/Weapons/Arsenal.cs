using System.Linq;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class Arsenal
    {
        public ArsenalData Origin { get; }
        public int OwnerId { get; }
        public ObservableList<Weapon> Weapons { get; } = new();

        public Arsenal(ArsenalData data)
        {
            Origin = data;
            OwnerId = data.OwnerId;
            data.Weapons.ForEach(weaponData => Weapons.Add(new Weapon(weaponData)));

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