using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Weapons;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands
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