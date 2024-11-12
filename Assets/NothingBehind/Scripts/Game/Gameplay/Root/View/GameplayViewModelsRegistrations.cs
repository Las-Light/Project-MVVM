using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Services;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public static class GameplayViewModelsRegistrations
    {
        public static void Register(DIContainer container)
        {
            container.RegisterFactory(c => new UIGameplayRootViewModel()).AsSingle();
            container.RegisterFactory(c => new WorldGameplayRootViewModel(c
                .Resolve<CharactersService>())).AsSingle();
        }
    }
}