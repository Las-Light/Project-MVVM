using DI.Scripts;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;
using R3;

namespace NothingBehind.Scripts.Game.GlobalMap.Root
{
    public static class GlobalMapRegistrations
    {
        public static void Register(DIContainer container, SceneEnterParams enterParams)
        {
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var gameState = gameStateProvider.GameState;
            var globalMap = gameState.GlobalMap;
            var settingsProvider = container.Resolve<ISettingsProvider>();
            var gameSettings = settingsProvider.GameSettings;
            
            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<GlobalMapExitParams>());
            
            // регистрируем процессор и команды, а также кладём CommandProcessor в контейнер

            var commandProcessor = container.Resolve<ICommandProcessor>();
           
            // регистрируем менеджеры

            var inputManager = container.Resolve<InputManager>();
            
            // регистрируем сервисы

            var equipmentService = container.Resolve<EquipmentService>();
        }
    }
}