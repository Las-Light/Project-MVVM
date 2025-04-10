using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Weapons
{
    public class CmdAddWeaponToArsenal : ICommand
    {
        public readonly int OwnerId;
        public readonly Weapon Weapon;

        public CmdAddWeaponToArsenal(int ownerId, Weapon weapon)
        {
            OwnerId = ownerId;
            Weapon = weapon;
        }
    }
}