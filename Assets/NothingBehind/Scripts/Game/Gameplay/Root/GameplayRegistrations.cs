using System.Linq;
using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Commands.Handlers;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;

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
            
            // регистрируем процессор и команды, а также кладём CommandProcessor в контейнер
            var commandProcessor = new CommandProcessor(gameStateProvider);
            commandProcessor.RegisterHandler(new CmdCreateCharacterHandler(gameState));
            commandProcessor.RegisterHandler(new CmdCreateMapStateHandler(gameState, gameSettings));
            container.RegisterInstance<ICommandProcessor>(commandProcessor);
            
            // регистрируем сервисы

            container.RegisterFactory(c => new InitialMapStateService(gameStateProvider, commandProcessor, enterParams)).AsSingle();

            container.Resolve<InitialMapStateService>();
            var currentMap = enterParams.MapId;
            var loadingMap = gameState.Maps.First(m => m.Id == currentMap);
            
            container.RegisterFactory(c => new CharactersService(
                loadingMap.Characters,
                gameSettings.CharactersSettings,
                commandProcessor)
            ).AsSingle();
        }
    }
}