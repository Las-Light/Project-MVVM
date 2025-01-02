using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers;
using NothingBehind.Scripts.Game.Gameplay.Services;
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
            var charsctersSettings = gameSettings.CharactersSettings;
            
            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<GameplayExitParams>());
            
            // регистрируем процессор и команды, а также кладём CommandProcessor в контейнер
            var commandProcessor = new CommandProcessor(gameStateProvider);
            commandProcessor.RegisterHandler(new CmdCreateCharacterHandler(gameState, charsctersSettings));
            commandProcessor.RegisterHandler(new CmdCreateMapStateHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdResourcesAddHandler(gameState));
            commandProcessor.RegisterHandler(new CmdResourcesSpendHandler(gameState));
            container.RegisterInstance<ICommandProcessor>(commandProcessor);
            
            // регистрируем сервисы


            // var loadingMap = GetLoadingMap(container, enterParams, gameState);
            container.RegisterFactory(c => new InitialMapStateService(
                gameStateProvider,
                commandProcessor,
                enterParams)).AsSingle();

            var loadingMap = container.Resolve<InitialMapStateService>().LoadingMap;

            container.RegisterFactory(c => new CharactersService(
                loadingMap.Characters,
                gameSettings.CharactersSettings,
                commandProcessor)
            ).AsSingle();

            container.RegisterFactory(c => new SpawnService(
                loadingMap,
                container.Resolve<CharactersService>()));

            container.RegisterFactory(c => new ResourcesService(gameState.Resources, commandProcessor));
        }

        // private static Map GetLoadingMap(DIContainer container, SceneEnterParams enterParams, GameStateProxy gameState)
        // {
        //     container.Resolve<InitialMapStateService>();
        //     var currentMap = enterParams.MapId;
        //     var loadingMap = gameState.Maps.First(m => m.Id == currentMap);
        //     return loadingMap;
        // }
    }
}