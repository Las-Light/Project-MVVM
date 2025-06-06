using System.Linq;
using DI.Scripts;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.ArsenalHandlers;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.CharactersHandlers;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.StoragesHandlers;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Utils;
using R3;

namespace NothingBehind.Scripts.Game.BattleGameplay.Root
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
            var playerSettings = gameSettings.PlayerSettings;
            var inventoriesSettings = gameSettings.InventoriesSettings;
            var equipmentsSettings = gameSettings.EquipmentsSettings;
            var storagesSettings = gameSettings.StoragesSettings;
            var itemsSettings = gameSettings.ItemsSettings;
            var weaponSettings = gameSettings.WeaponsSettings;
            var coroutines = container.Resolve<Coroutines>(AppConstants.COROUTINES);
            var equipmentService = container.Resolve<EquipmentService>();
            var inventoryService = container.Resolve<InventoryService>();

            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<GameplayExitParams>());
            container.RegisterInstance(AppConstants.EXIT_INVENTORY_REQUEST_TAG, new Subject<ExitInventoryRequestResult>());

            // регистрируем команды
            var commandProcessor = container.Resolve<ICommandProcessor>();
            commandProcessor.RegisterHandler(new CmdCreateCharacterHandler(gameState, charactersSettings));
            commandProcessor.RegisterHandler(new CmdRemoveCharacterHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateStorageHandler(gameState, storagesSettings));
            commandProcessor.RegisterHandler(new CmdRemoveStorageHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateArsenalHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdRemoveArsenalHandler(gameState));
            commandProcessor.RegisterHandler(new CmdAddWeaponToArsenalHandler(gameState));
            commandProcessor.RegisterHandler(new CmdRemoveWeaponFromArsenalHandler(gameState));
            commandProcessor.RegisterHandler(new CmdTriggeredEnemySpawnHandler(gameState));

            // регистрируем менеджеры

            var inputManager = container.Resolve<InputManager>();

            // регистрируем сервисы

            container.RegisterFactory(c => new CameraService(
                inputManager,
                gameplayCameraSettings)).AsSingle();
            
            container.RegisterFactory(c => new ArsenalService(gameState.Arsenals,
                equipmentService,
                inventoryService,
                weaponSettings, 
                commandProcessor)).AsSingle();
            var weaponService = container.Resolve<ArsenalService>();
            
            var loadingMap = gameState.Maps.First(m => m.Id == enterParams.TargetMapId);

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