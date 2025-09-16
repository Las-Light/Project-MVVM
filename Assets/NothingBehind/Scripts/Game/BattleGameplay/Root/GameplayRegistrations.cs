using System.Linq;
using DI.Scripts;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.ArsenalHandlers;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.EntityHandlers;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.EquipmentHandlers;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.Handlers.InventoriesHandlers;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.Services;
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
            var entitiesSettings = gameSettings.EntitiesSettings;
            var charactersSettings = entitiesSettings.CharactersSettings;
            var gameplayCameraSettings = gameSettings.GameplayCameraSettings;
            var inventoriesSettings = gameSettings.InventoriesSettings;
            var equipmentsSettings = gameSettings.EquipmentsSettings;
            var storagesSettings = gameSettings.EntitiesSettings.StoragesSettings;
            var itemsSettings = gameSettings.ItemsSettings;
            var weaponSettings = gameSettings.WeaponsSettings;
            var coroutines = container.Resolve<Coroutines>(AppConstants.COROUTINES);

            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<GameplayExitParams>());
            container.RegisterInstance(AppConstants.EXIT_INVENTORY_REQUEST_TAG, new Subject<ExitInventoryRequestResult>());

            // регистрируем команды
            var commandProcessor = container.Resolve<ICommandProcessor>();
            commandProcessor.RegisterHandler(new CmdCreateEntityHandler(gameState, gameSettings));
            commandProcessor.RegisterHandler(new CmdRemoveEntityHandler(gameState));
            commandProcessor.RegisterHandler(new CmdAddGridToInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdRemoveGridInventoryHandler(gameState));
            commandProcessor.RegisterHandler(new CmdEquipItemHandler());
            commandProcessor.RegisterHandler(new CmdUnequipItemHandler());
            commandProcessor.RegisterHandler(new CmdAddWeaponToArsenalHandler(gameState));
            commandProcessor.RegisterHandler(new CmdRemoveWeaponFromArsenalHandler(gameState));
            commandProcessor.RegisterHandler(new CmdTriggeredEnemySpawnHandler(gameState));

            // регистрируем менеджеры

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

            container.RegisterFactory(c => new ArsenalService(gameState.Player, loadingMap.Entities,
                equipmentService,
                inventoryService,
                weaponSettings, 
                commandProcessor)).AsSingle();
            var weaponService = container.Resolve<ArsenalService>();
            
            container.RegisterFactory(c => new StorageService(
                loadingMap.Entities,
                storagesSettings,
                inventoryService,
                commandProcessor,
                container.Resolve<Subject<ExitInventoryRequestResult>>(AppConstants.EXIT_INVENTORY_REQUEST_TAG)))
                .AsSingle();

            container.RegisterFactory(c => new CharactersService(
                loadingMap.Entities,
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