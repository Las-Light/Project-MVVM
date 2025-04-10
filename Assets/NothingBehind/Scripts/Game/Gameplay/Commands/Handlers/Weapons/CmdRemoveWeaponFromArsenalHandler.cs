using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.Weapons;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Weapons
{
    public class CmdRemoveWeaponFromArsenalHandler : ICommandHandler<CmdRemoveWeaponFromArsenal>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveWeaponFromArsenalHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public CommandResult Handle(CmdRemoveWeaponFromArsenal command)
        {
            var arsenal = _gameState.Arsenals.FirstOrDefault(arsenal => arsenal.OwnerId == command.OwnerId);
            if (arsenal != null)
            {
                var removedWeapon = arsenal.Weapons.FirstOrDefault(weapon => weapon.Id == command.WeaponId);
                if (removedWeapon != null)
                {
                    arsenal.Weapons.Remove(removedWeapon);
                    return new CommandResult(removedWeapon.Id, true);
                }

                Debug.LogError(
                    $"Weapon with Id - {command.WeaponId} is not found in arsenal with ownerId - {command.OwnerId}");
                return new CommandResult(command.WeaponId, false);
            }

            Debug.LogError($"Couldn't find arsenal with ownerId - {command.OwnerId}");
            return new CommandResult(command.WeaponId, false);
        }
    }
}