using System.Linq;
using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.CharactersHandlers;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.EquipmentHandlers;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.InventoriesHandlers;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.PlayerHandlers;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.StoragesHandlers;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Weapons;
using NothingBehind.Scripts.Game.Gameplay.Commands.Weapons;
using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Utils;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public static class GameplayRegistrations
    {
        public static void Register(DIContainer container, SceneEnterParams enterParams)
        {
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var gameState = gameStateProvider.GameState;
            var settingsProvider = container.Resolve<ISettingsProvider>();
            var gameSettings = settingsProvider.GameSettings;
            var charactersSettings = gameSettings.CharactersSettings;
            var gameplayCameraSettings = gameSettings.GameplayCameraSettings;
            var heroSettings = gameSettings.PlayerSettings;
            var inventoriesSettings = gameSettings.InventoriesSettings;
            var equipmentsSettings = gameSettings.EquipmentsSettings;
            var storagesSettings = gameSettings.StoragesSettings;
            var itemsSettings = gameSettings.ItemsSettings;
            var weaponSettings = gameSettings.WeaponsSettings;
            var coroutines = container.Resolve<Coroutines>(AppConstants.COROUTINES);

            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<GameplayExitParams>());
            container.RegisterInstance(AppConstants.EXIT_INVENTORY_REQUEST_TAG, new Subject<ExitInventoryRequestResult>());

            // регистрируем процессор и команды, а также кладём CommandProcessor в контейнер
            var commandProcessor = new CommandProcessor(gameStateProvider);
            commandProcessor.RegisterHandler(new CmdInitPlayerPosOnMapHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdCreateCharacterHandler(gameState, charactersSettings));
            commandProcessor.RegisterHandler(new CmdRemoveCharacterHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateStorageHandler(gameState, storagesSettings));
            commandProcessor.RegisterHandler(new CmdRemoveStorageHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateArsenalHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdRemoveArsenalHandler(gameState));
            commandProcessor.RegisterHandler(new CmdAddWeaponToArsenalHandler(gameState));
            commandProcessor.RegisterHandler(new CmdRemoveWeaponFromArsenalHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateInventoryHandler(gameState, inventoriesSettings));
            commandProcessor.RegisterHandler(new CmdRemoveInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdAddGridToInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdRemoveGridInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateEquipmentHandler(gameState, equipmentsSettings, gameSettings));
            commandProcessor.RegisterHandler(new CmdRemoveEquipmentHandler(gameState));
            commandProcessor.RegisterHandler(new CmdResourcesAddHandler(gameState));
            commandProcessor.RegisterHandler(new CmdResourcesSpendHandler(gameState));
            commandProcessor.RegisterHandler(new CmdTriggeredEnemySpawnHandler(gameState));
            commandProcessor.RegisterHandler(new CmdUpdatePlayerPosOnMapHandler(gameState));
            container.RegisterInstance<ICommandProcessor>(commandProcessor);

            // регистрируем менеджеры

            container.RegisterFactory(c => new GameplayInputManager()).AsSingle();
            var inputManager = container.Resolve<GameplayInputManager>();
            
            container.RegisterFactory(c => new CameraManager(
                inputManager,
                coroutines,
                gameplayCameraSettings)).AsSingle();
            var cameraService = container.Resolve<CameraManager>();

            container.RegisterFactory(c =>
                new PlayerMovementManager(heroSettings, inputManager)).AsSingle();
            
            container.RegisterFactory(c => new PlayerTurnManager(inputManager, heroSettings)).AsSingle();
            
            
            // регистрируем сервисы
            container.RegisterFactory(c => new EquipmentService(gameState.Equipments, itemsSettings,
                commandProcessor)).AsSingle();
            var equipmentService = container.Resolve<EquipmentService>();
            
            container.RegisterFactory(c =>
                new InventoryService(gameState.Inventories, 
                    equipmentService,
                    inventoriesSettings,
                    itemsSettings,
                    commandProcessor, 
                    gameState.Player.Value.Id))
                .AsSingle();
            var inventoryService = container.Resolve<InventoryService>();

            container.RegisterFactory(c => new ArsenalService(gameState.Arsenals, equipmentService, inventoryService,
                weaponSettings, commandProcessor)).AsSingle();
            var weaponService = container.Resolve<ArsenalService>();
            
            container.RegisterFactory(c => new PlayerService(
                    inventoryService,
                    container.Resolve<PlayerMovementManager>(),
                    container.Resolve<PlayerTurnManager>(),
                    weaponService,
                    gameState.Player.Value,
                    commandProcessor,
                    enterParams))
                .AsSingle();

            container.RegisterFactory(c => new ResourcesService(gameState.Resources, commandProcessor)).AsSingle();

            var loadingMap = gameState.Maps.First(m => m.Id == enterParams.TargetMapId);

            container.RegisterFactory(c => new MapTransferService(
                loadingMap.MapTransfers)).AsSingle();

            container.RegisterFactory(c => new StorageService(
                loadingMap.Storages,
                storagesSettings,
                inventoryService,
                commandProcessor,
                container.Resolve<Subject<ExitInventoryRequestResult>>(AppConstants.EXIT_INVENTORY_REQUEST_TAG)))
                .AsSingle();

            container.RegisterFactory(c => new CharactersService(
                loadingMap.Characters,
                charactersSettings,
                equipmentService,
                inventoryService,
                weaponService,
                commandProcessor,
                container.Resolve<Subject<ExitInventoryRequestResult>>(AppConstants.EXIT_INVENTORY_REQUEST_TAG))
            ).AsSingle();

            container.RegisterFactory(c => new SpawnService(
                loadingMap.EnemySpawns,
                container.Resolve<CharactersService>(),
                commandProcessor)).AsSingle();
        }
    }
}