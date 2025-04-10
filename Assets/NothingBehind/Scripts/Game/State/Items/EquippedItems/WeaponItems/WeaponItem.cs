using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;

namespace NothingBehind.Scripts.Game.State.Items.EquippedItems.WeaponItems
{
    public class WeaponItem : Item
    {
        public Weapon Weapon { get; set; }
        public WeaponItem(WeaponItemData weaponItemData) : base(weaponItemData)
        {
            Weapon = new Weapon(weaponItemData.WeaponData);
        }
    }
}