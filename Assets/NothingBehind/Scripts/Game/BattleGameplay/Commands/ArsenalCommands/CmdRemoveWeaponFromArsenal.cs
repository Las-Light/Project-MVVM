using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands
{
    public class CmdRemoveWeaponFromArsenal : ICommand
    {
        public readonly int OwnerId;
        public readonly int WeaponId;

        public CmdRemoveWeaponFromArsenal(int ownerId, int weaponId)
        {
            OwnerId = ownerId;
            WeaponId = weaponId;
        }
    }
}