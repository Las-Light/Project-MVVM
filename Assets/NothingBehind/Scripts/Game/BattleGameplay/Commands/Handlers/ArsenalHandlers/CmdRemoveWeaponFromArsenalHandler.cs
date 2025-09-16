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
    public class CmdRemoveWeaponFromArsenalHandler : ICommandHandler<CmdRemoveWeaponFromArsenal>
    {
        private readonly GameStateProxy _gameState;

        public CmdRemoveWeaponFromArsenalHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public CommandResult Handle(CmdRemoveWeaponFromArsenal command)
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
                    Debug.Log($"Couldn't find arsenal at Entity with ID: {command.OwnerId} and EntityType: {command.OwnerType}");
                    return new CommandResult(false);
                }
            }
            
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