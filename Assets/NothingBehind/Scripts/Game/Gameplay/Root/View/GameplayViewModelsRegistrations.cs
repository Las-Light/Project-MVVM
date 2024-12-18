using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.State;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public static class GameplayViewModelsRegistrations
    {
        public static void Register(DIContainer container)
        {
            container.RegisterFactory(c => new UIGameplayRootViewModel()).AsSingle();
            container.RegisterFactory(c => new WorldGameplayRootViewModel(
                    c.Resolve<CharactersService>(),
                    c.Resolve<IGameStateProvider>(),
                    c.Resolve<ResourcesService>(),
                    c.Resolve<InitialMapStateService>()))
                .AsSingle();
        }
    }
}