using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
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
                    c.Resolve<GameplayInputManager>(),
                    c.Resolve<CameraManager>(),
                    c.Resolve<InventoryService>()))
                .AsSingle();
        }
    }
}