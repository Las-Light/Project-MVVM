using DI.Scripts;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.MainMenu.Services;

namespace NothingBehind.Scripts.Game.MainMenu.Root
{
    public static class MainMenuRegistrations
    {
        public static void Register(DIContainer container, SceneEnterParams sceneEnterParams)
        {
            container.RegisterFactory(c => new SomeMainMenuService(c.Resolve<InitialGameStateService>())).AsSingle();
        }
    }
}