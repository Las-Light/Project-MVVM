using DI.Scripts;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.UI;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Root.View
{
    public static class GameplayViewModelsRegistrations
    {
        public static void Register(DIContainer container)
        {
            container.RegisterFactory(c => new GameplayUIManager(container)).AsSingle();
            container.RegisterFactory(c => new UIGameplayRootViewModel()).AsSingle();
            container.RegisterFactory(c => new WorldGameplayRootViewModel(
                    c.Resolve<ISettingsProvider>(),
                    c.Resolve<CharactersService>(),
                    c.Resolve<StorageService>(),
                    c.Resolve<IGameStateProvider>(),
                    c.Resolve<PlayerService>(),
                    c.Resolve<ResourcesService>(),
                    c.Resolve<SpawnService>(),
                    c.Resolve<MapTransferService>(),
                    c.Resolve<InputManager>(),
                    c.Resolve<CameraService>(),
                    c.Resolve<InventoryService>(),
                    c.Resolve<EquipmentService>(),
                    c.Resolve<ArsenalService>()))
                .AsSingle();
        }
    }
}