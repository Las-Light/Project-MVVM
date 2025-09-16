using System.Collections.Generic;
using DI.Scripts;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.PlayerHandlers;
using NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.ResourcesHandlers;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using ObservableCollections;

namespace NothingBehind.Scripts.Game.GameRoot
{
    public static class RootServicesRegistrations
    {
        public static void Register(DIContainer container)
        {
            var settingsProvider = container.Resolve<ISettingsProvider>();
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var commandProcessor = container.Resolve<ICommandProcessor>();

            var gameState = gameStateProvider.GameState;
            var gameSettings = settingsProvider.GameSettings;
            var charactersSettings = gameSettings.EntitiesSettings;
            var gameplayCameraSettings = gameSettings.GameplayCameraSettings;
            var playerSettings = gameSettings.EntitiesSettings.PlayerSettings;
            var inventoriesSettings = gameSettings.InventoriesSettings;
            var equipmentsSettings = gameSettings.EquipmentsSettings;
            var storagesSettings = gameSettings.EntitiesSettings.StoragesSettings;
            var itemsSettings = gameSettings.ItemsSettings;
            var weaponSettings = gameSettings.WeaponsSettings;

            // регистрируем команды

            commandProcessor.RegisterHandler(new CmdInitPlayerPosOnMapHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdUpdatePlayerPosOnMapHandler(gameState));
            commandProcessor.RegisterHandler(new CmdResourcesAddHandler(gameState));
            commandProcessor.RegisterHandler(new CmdResourcesSpendHandler(gameState));


            // регистрируем менеджеры
            var inputManager = container.Resolve<InputManager>();

            // регистрируем общие сервисы

            if (!container.IsRegistered<CameraService>())
            {
                container.RegisterFactory(c => new CameraService(inputManager, gameplayCameraSettings)).AsSingle();
            }
            
            if (!container.IsRegistered<MapTransferService>())
            {
                Dictionary<MapId, ObservableList<MapTransferData>> transferMaps = new();
                foreach (var map in gameState.Maps)
                {
                    transferMaps[map.Id] = map.MapTransfers;
                }

                container.RegisterFactory(c => new MapTransferService(transferMaps)).AsSingle();
            }

            if (!container.IsRegistered<ResourcesService>())
                container.RegisterFactory(c => new ResourcesService(gameState.Resources, commandProcessor)).AsSingle();
            else
                container.Resolve<ResourcesService>().UpdateResourcesService(gameState.Resources);

            if (!container.IsRegistered<PlayerService>())
                container.RegisterFactory(c => new PlayerService(
                    gameState.Player.Value,
                    inputManager,
                    commandProcessor,
                    playerSettings)).AsSingle();
            else
                container.Resolve<PlayerService>().UpdatePlayerService(gameState.Player.Value);

        }
    }
}