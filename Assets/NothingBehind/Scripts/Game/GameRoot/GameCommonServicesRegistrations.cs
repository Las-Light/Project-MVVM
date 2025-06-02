using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.GameRoot
{
    public static class GameCommonServicesRegistrations
    {
        public static void Register(DIContainer container)
        {
            var settingsProvider = container.Resolve<ISettingsProvider>();
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var commandProcessor = container.Resolve<ICommandProcessor>();
            
            var gameState = gameStateProvider.GameState;
            var gameSettings = settingsProvider.GameSettings;
            var charactersSettings = gameSettings.CharactersSettings;
            var gameplayCameraSettings = gameSettings.GameplayCameraSettings;
            var playerSettings = gameSettings.PlayerSettings;
            var inventoriesSettings = gameSettings.InventoriesSettings;
            var equipmentsSettings = gameSettings.EquipmentsSettings;
            var storagesSettings = gameSettings.StoragesSettings;
            var itemsSettings = gameSettings.ItemsSettings;
            var weaponSettings = gameSettings.WeaponsSettings;

            if (!container.IsRegistered<EquipmentService>())
            {
                container.RegisterFactory(c => new EquipmentService(gameState.Equipments, itemsSettings,
                    commandProcessor)).AsSingle();
            }
            var equipmentService = container.Resolve<EquipmentService>();

            if (!container.IsRegistered<InventoryService>())
            {
                container.RegisterFactory(c =>
                        new InventoryService(gameState.Inventories, 
                            equipmentService,
                            inventoriesSettings,
                            itemsSettings,
                            commandProcessor, 
                            gameState.Player.Value.Id))
                    .AsSingle();
            }
        }
    }
}