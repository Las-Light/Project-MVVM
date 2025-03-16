using System.Linq;
using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Hero;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers.Inventories;
using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.Logic.Hero;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.Services.Hero;
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
            var heroSettings = gameSettings.playerSettings;
            var inventoriesSettings = gameSettings.InventoriesSettings;
            var coroutines = container.Resolve<Coroutines>(AppConstants.COROUTINES);

            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<GameplayExitParams>());

            // регистрируем процессор и команды, а также кладём CommandProcessor в контейнер
            var commandProcessor = new CommandProcessor(gameStateProvider);
            commandProcessor.RegisterHandler(new CmdInitHeroPosOnMapHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdCreateCharacterHandler(gameState, charactersSettings));
            commandProcessor.RegisterHandler(new CmdCreateInventoryHandler(gameState, inventoriesSettings));
            commandProcessor.RegisterHandler(new CmdRemoveInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateGridInventoryHandler(gameState, inventoriesSettings));
            commandProcessor.RegisterHandler(new CmdRemoveGridInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdResourcesAddHandler(gameState));
            commandProcessor.RegisterHandler(new CmdResourcesSpendHandler(gameState));
            commandProcessor.RegisterHandler(new CmdTriggeredEnemySpawnHandler(gameState));
            commandProcessor.RegisterHandler(new CmdUpdateHeroPosOnMapHandler(gameState));
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
                new HeroMovementManager(heroSettings, inputManager)).AsSingle();
            
            container.RegisterFactory(c => new HeroTurnManager(inputManager, heroSettings)).AsSingle();
            
            
            // регистрируем сервисы

            container.RegisterFactory(c =>
                new InventoryService(gameState.Inventories, 
                    inventoriesSettings, 
                    commandProcessor, 
                    gameState.Hero.Value.Id))
                .AsSingle();
            var inventoryService = container.Resolve<InventoryService>();
            container.RegisterFactory(c => new HeroService(
                    inventoryService,
                    container.Resolve<HeroMovementManager>(),
                    container.Resolve<HeroTurnManager>(),
                    gameState,
                    commandProcessor,
                    enterParams))
                .AsSingle();

            container.RegisterFactory(c => new ResourcesService(gameState.Resources, commandProcessor)).AsSingle();

            var loadingMap = gameState.Maps.First(m => m.Id == enterParams.TargetMapId);

            container.RegisterFactory(c => new MapTransferService(
                loadingMap.MapTransfers)).AsSingle();

            container.RegisterFactory(c => new CharactersService(
                loadingMap.Characters,
                gameSettings.CharactersSettings,
                inventoryService,
                commandProcessor)
            ).AsSingle();

            container.RegisterFactory(c => new SpawnService(
                loadingMap.EnemySpawns,
                container.Resolve<CharactersService>(),
                commandProcessor)).AsSingle();
        }
    }
}