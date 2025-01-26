using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.Services.Hero;
using NothingBehind.Scripts.Game.Gameplay.Services.InputManager;
using NothingBehind.Scripts.Game.Gameplay.View.UI;
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
                    c.Resolve<CharactersService>(),
                    c.Resolve<IGameStateProvider>(),
                    c.Resolve<HeroService>(),
                    c.Resolve<ResourcesService>(),
                    c.Resolve<SpawnService>(),
                    c.Resolve<MapTransferService>(),
                    c.Resolve<GameplayInputManager>(),
                    c.Resolve<CameraService>()))
                .AsSingle();
        }
    }
}