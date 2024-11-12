using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Commands;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public static class GameplayRegistrations
    {
        public static void Register(DIContainer container, GameplayEnterParams gameplayEnterParams)
        {
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var gameState = gameStateProvider.GameState;
            var settingsProvider = container.Resolve<ISettingsProvider>();
            var gameSettings = settingsProvider.GameSettings;
            
            // регистрируем процессор и команды, а также кладём CommandProcessor в контейнер
            var commandProcessor = new CommandProcessor(gameStateProvider);
            commandProcessor.RegisterHandler(new CmdCreateCharacterHandler(gameState));
            container.RegisterInstance<ICommandProcessor>(commandProcessor);
            
            // регистрируем сервис
            container.RegisterFactory(c => new CharactersService(
                gameState.Characters,
                gameSettings.CharactersSettings,
                commandProcessor)
            ).AsSingle();
        }
    }
}