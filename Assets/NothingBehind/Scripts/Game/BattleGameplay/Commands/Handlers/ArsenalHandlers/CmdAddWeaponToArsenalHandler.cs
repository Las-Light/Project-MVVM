using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.ArsenalHandlers
{
    public class CmdAddWeaponToArsenalHandler : ICommandHandler<CmdAddWeaponToArsenal>
    {
        private readonly GameStateProxy _gameState;

        public CmdAddWeaponToArsenalHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }
        public CommandResult Handle(CmdAddWeaponToArsenal command)
        {
            var arsenal = _gameState.Arsenals.FirstOrDefault(arsenal => arsenal.OwnerId == command.OwnerId);
            if (arsenal != null)
            {
                var addedWeapon = arsenal.Weapons.FirstOrDefault(weapon => weapon.Id == command.Weapon.Id);
                if (addedWeapon != null)
                {
                    Debug.LogError($"Weapon with Id - {addedWeapon.Id} is already exists in arsenal with ownerId - {command.OwnerId}");
                    return new CommandResult(addedWeapon.Id, false);
                }
                arsenal.Weapons.Add(command.Weapon);
                return new CommandResult(command.Weapon.Id, true);
            }
            Debug.LogError($"Couldn't find arsenal with ownerId - {command.OwnerId}");
            return new CommandResult(command.Weapon.Id, false);
        }
    }
}