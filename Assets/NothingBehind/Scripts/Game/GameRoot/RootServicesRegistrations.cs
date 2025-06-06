using System.Collections.Generic;
using DI.Scripts;
using NothingBehind.Scripts.Game.BattleGameplay.Root.Commands.Handlers.PlayerHandlers;
using NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.EquipmentHandlers;
using NothingBehind.Scripts.Game.GameRoot.Commands.Handlers.InventoriesHandlers;
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
            var charactersSettings = gameSettings.CharactersSettings;
            var gameplayCameraSettings = gameSettings.GameplayCameraSettings;
            var playerSettings = gameSettings.PlayerSettings;
            var inventoriesSettings = gameSettings.InventoriesSettings;
            var equipmentsSettings = gameSettings.EquipmentsSettings;
            var storagesSettings = gameSettings.StoragesSettings;
            var itemsSettings = gameSettings.ItemsSettings;
            var weaponSettings = gameSettings.WeaponsSettings;

            // регистрируем команды

            commandProcessor.RegisterHandler(new CmdInitPlayerPosOnMapHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdUpdatePlayerPosOnMapHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateInventoryHandler(gameState, inventoriesSettings));
            commandProcessor.RegisterHandler(new CmdRemoveInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdAddGridToInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdRemoveGridInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateEquipmentHandler(gameState, equipmentsSettings,
                gameSettings));
            commandProcessor.RegisterHandler(new CmdRemoveEquipmentHandler(gameState));
            commandProcessor.RegisterHandler(new CmdEquipItemHandler());
            commandProcessor.RegisterHandler(new CmdUnequipItemHandler());
            commandProcessor.RegisterHandler(new CmdResourcesAddHandler(gameState));
            commandProcessor.RegisterHandler(new CmdResourcesSpendHandler(gameState));


            // регистрируем менеджеры
            var inputManager = container.Resolve<InputManager>();

            // регистрируем общие сервисы
            
            if (!container.IsRegistered<MapTransferService>())
            {
                Dictionary<MapId, ObservableList<MapTransferData>> transferMaps = new();
                transferMaps[gameState.GlobalMap.Value.MapId] = gameState.GlobalMap.Value.MapTransfers;
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

            if (!container.IsRegistered<EquipmentService>())
                container.RegisterFactory(c => new EquipmentService(
                    gameState.Equipments,
                    itemsSettings,
                    commandProcessor)).AsSingle();
            else
                container.Resolve<EquipmentService>().UpdateEquipmentService(gameState.Equipments);

            var equipmentService = container.Resolve<EquipmentService>();

            if (!container.IsRegistered<InventoryService>())
                container.RegisterFactory(c =>
                        new InventoryService(gameState.Inventories,
                            equipmentService,
                            inventoriesSettings,
                            itemsSettings,
                            commandProcessor,
                            gameState.Player.Value.Id))
                    .AsSingle();
            else
                container.Resolve<InventoryService>().UpdateInventories(gameState.Inventories);
        }
    }
}