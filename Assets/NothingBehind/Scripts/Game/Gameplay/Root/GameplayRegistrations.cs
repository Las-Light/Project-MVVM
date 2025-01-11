using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.Services.InputManager;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;
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

            var inputManager = container.RegisterFactory(c => new GameplayInputManager()).AsSingle();
            
            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<GameplayExitParams>());
            
            // регистрируем процессор и команды, а также кладём CommandProcessor в контейнер
            var commandProcessor = new CommandProcessor(gameStateProvider);
            commandProcessor.RegisterHandler(new CmdCreateHeroHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdCreateCharacterHandler(gameState, charactersSettings));
            commandProcessor.RegisterHandler(new CmdCreateMapStateHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdResourcesAddHandler(gameState));
            commandProcessor.RegisterHandler(new CmdResourcesSpendHandler(gameState));
            commandProcessor.RegisterHandler(new CmdTriggeredEnemySpawnHandler(gameState));
            container.RegisterInstance<ICommandProcessor>(commandProcessor);
            
            // регистрируем сервисы

            container.RegisterFactory(c => new HeroService(gameState, commandProcessor, enterParams)).AsSingle();
            
            container.RegisterFactory(c => new InitialMapStateService(
                gameStateProvider,
                commandProcessor,
                enterParams)).AsSingle();

            //попробовать вместо инициализации карты сделать передачу loadingMap из enterParams и gameSettings
            //(или полная инициализация из настроек)
            var loadingMap = container.Resolve<InitialMapStateService>().LoadingMap;

            container.RegisterFactory(c => new CharactersService(
                loadingMap.Characters,
                gameSettings.CharactersSettings,
                commandProcessor)
            ).AsSingle();

            container.RegisterFactory(c => new SpawnService(
                loadingMap,
                container.Resolve<CharactersService>(),
                commandProcessor));

            container.RegisterFactory(c => new ResourcesService(gameState.Resources, commandProcessor));
        }
    }
}