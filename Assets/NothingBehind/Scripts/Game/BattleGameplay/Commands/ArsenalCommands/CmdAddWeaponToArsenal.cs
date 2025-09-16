using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Weapons;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands
{
    public class CmdAddWeaponToArsenal : ICommand
    {
        public readonly EntityType OwnerType;
        public readonly int OwnerId;
        public readonly Weapon Weapon;

        public CmdAddWeaponToArsenal(EntityType ownerType, int ownerId, Weapon weapon)
        {
            OwnerType = ownerType;
            OwnerId = ownerId;
            Weapon = weapon;
        }
    }
}