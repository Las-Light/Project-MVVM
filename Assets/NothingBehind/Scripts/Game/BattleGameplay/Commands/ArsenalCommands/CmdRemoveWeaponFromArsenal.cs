using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands
{
    public class CmdRemoveWeaponFromArsenal : ICommand
    {
        public readonly EntityType OwnerType;
        public readonly int OwnerId;
        public readonly int WeaponId;

        public CmdRemoveWeaponFromArsenal(EntityType ownerType, int ownerId, int weaponId)
        {
            OwnerType = ownerType;
            OwnerId = ownerId;
            WeaponId = weaponId;
        }
    }
}