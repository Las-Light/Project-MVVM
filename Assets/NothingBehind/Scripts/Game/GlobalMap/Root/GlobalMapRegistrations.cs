using System.Linq;
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
            var settingsProvider = container.Resolve<ISettingsProvider>();
            var gameSettings = settingsProvider.GameSettings;
            var itemsSettings = gameSettings.ItemsSettings;
            var inventoriesSettings = gameSettings.InventoriesSettings;
            var commandProcessor = container.Resolve<ICommandProcessor>();
            
            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<GlobalMapExitParams>());
            
            // регистрируем процессор и команды, а также кладём CommandProcessor в контейнер

           
            // регистрируем менеджеры

            //var inputManager = container.Resolve<InputManager>();
            
            // регистрируем сервисы
            var loadingMap = gameState.Maps.First(m => m.Id == enterParams.TargetMapId);
            
            container.RegisterFactory(c => new EquipmentService(gameState.Player, loadingMap.Entities, 
                    itemsSettings,
                    commandProcessor))
                .AsSingle();
            var equipmentService = container.Resolve<EquipmentService>();

            container.RegisterFactory(c => new InventoryService(gameState.Player, loadingMap.Entities,
                    equipmentService,
                    inventoriesSettings,
                    itemsSettings,
                    commandProcessor))
                .AsSingle();
            var inventoryService = container.Resolve<InventoryService>();

            //var equipmentService = container.Resolve<EquipmentService>();
        }
    }
}