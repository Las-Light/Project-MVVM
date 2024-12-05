using System;
using System.Linq;
using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Commands;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public static class GameplayRegistrations
    {
        public static void Register(DIContainer container, SceneEnterParams gameplayEnterParams)
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
            
            // На данный момент мы знаем, что мы пытаемся загрузить карту. Но не знаем, есть ли ее состояние вообще.
            // Создание карты - это модель, так что работать с ней нужно через команды, поэтому нужен обработчик команд
            // на случай, если состояния карты еще не суествует. Может мы этот момент передалаем потом, чтобы 
            // состояние карты создавалось ДО загрузки сцены и тут не было подобных проверок, но пока так. Делаем пошагово
            //var loadingMapId = gameplayEnterParams.MapId;
            var loadingMapId = Scenes.GAMEPLAY;
            var loadingMap = gameState.Maps.FirstOrDefault(m => m.Id == loadingMapId);
            if (loadingMap == null)
            {
                // Создание состояния, если его еще нет через команду.
                var command = new CmdCreateMapState(loadingMapId);
                var success = commandProcessor.Process(command);
                if (!success)
                {
                    throw new Exception($"Couldn't create map state with id: ${loadingMapId}");
                }

                loadingMap = gameState.Maps.First(m => m.Id == loadingMapId);
            }
            
            // регистрируем сервис
            container.RegisterFactory(c => new CharactersService(
                loadingMap.Characters,
                gameSettings.CharactersSettings,
                commandProcessor)
            ).AsSingle();
        }
    }
}