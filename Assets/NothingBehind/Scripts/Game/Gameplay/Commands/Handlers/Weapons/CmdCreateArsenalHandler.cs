using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Commands.Weapons;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.Settings.Gameplay.Weapons;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Weapons
{
    public class CmdCreateArsenalHandler : ICommandHandler<CmdCreateArsenal>
    {
        private readonly GameStateProxy _gameState;
        private readonly GameSettings _gameSettings;

        public CmdCreateArsenalHandler(GameStateProxy gameState, GameSettings gameSettings)
        {
            _gameState = gameState;
            _gameSettings = gameSettings;
        }
        public CommandResult Handle(CmdCreateArsenal command)
        {
            var arsenalData = new ArsenalData
            {
                OwnerId = command.OwnerId,
                Weapons = new List<WeaponData>()
            };
            var weaponSettings =
                _gameSettings.WeaponsSettings.WeaponConfigs.First(settings => settings.WeaponType == WeaponType.Unarmed);
            var unarmedData = WeaponDataFactory.CreateWeaponData(_gameState.GameState, _gameSettings, _gameState.CreateItemId(), weaponSettings.WeaponName);
            arsenalData.Weapons.Add(unarmedData);
            _gameState.Arsenals.Add(new Arsenal(arsenalData));

            return new CommandResult(command.OwnerId, true);
        }
    }
}