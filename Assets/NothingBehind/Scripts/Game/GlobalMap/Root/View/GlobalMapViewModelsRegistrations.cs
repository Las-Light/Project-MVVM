using DI.Scripts;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.UI;

namespace NothingBehind.Scripts.Game.GlobalMap.Root.View
{
    public class GlobalMapViewModelsRegistrations
    {
        public static void Register(DIContainer container)
        {
            container.RegisterFactory(c => new WorldGlobalMapRootViewModel()).AsSingle();
            container.RegisterFactory(c => new GlobalMapUIManager(container)).AsSingle();
            container.RegisterFactory(c => new UIGlobalMapRootViewModel()).AsSingle();
        }
    }
}