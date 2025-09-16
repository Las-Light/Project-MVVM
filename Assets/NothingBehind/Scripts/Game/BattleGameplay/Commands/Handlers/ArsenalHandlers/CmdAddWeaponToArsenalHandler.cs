using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Game.State.Weapons;
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
            Arsenal arsenal;
            switch (command.OwnerType)
            {
                case EntityType.Player:
                    arsenal = _gameState.Player.CurrentValue.Arsenal.CurrentValue;
                    break;
                case EntityType.Character:
                {
                    var currentMap = _gameState.Maps.FirstOrDefault(m => m.Id == _gameState.CurrentMapId.CurrentValue);
                    if (currentMap == null)
                    {
                        Debug.Log($"Couldn't find MapState for ID: {_gameState.CurrentMapId.CurrentValue}");
                        return new CommandResult(false);
                    }

                    var entity = currentMap.Entities.FirstOrDefault(c => c.UniqueId == command.OwnerId);
                    if (entity is CharacterEntity characterEntity)
                    {
                        arsenal = characterEntity.Arsenal.CurrentValue;
                        break;
                    }
                    Debug.Log($"Couldn't find Character for ID: {command.OwnerId}");
                    return new CommandResult(false);
                }
                default:
                {
                    Debug.Log($"Couldn't add Weapon for Arsenal to Entity with ID: {command.OwnerId} and EntityType: {command.OwnerType}");
                    return new CommandResult(false);
                }
            }
            
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