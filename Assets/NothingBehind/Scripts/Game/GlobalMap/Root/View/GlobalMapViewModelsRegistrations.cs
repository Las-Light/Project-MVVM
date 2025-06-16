using DI.Scripts;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.UI;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;

namespace NothingBehind.Scripts.Game.GlobalMap.Root.View
{
    public class GlobalMapViewModelsRegistrations
    {
        public static void Register(DIContainer container)
        {
            container.RegisterFactory(c => new WorldGlobalMapRootViewModel(c.Resolve<ISettingsProvider>(),
                c.Resolve<IGameStateProvider>(), 
                c.Resolve<PlayerService>(),
                c.Resolve<ResourcesService>(),
                c.Resolve<MapTransferService>(),
                c.Resolve<InputManager>(),
                c.Resolve<CameraService>(),
                c.Resolve<InventoryService>())).AsSingle();
            container.RegisterFactory(c => new GlobalMapUIManager(container)).AsSingle();
            container.RegisterFactory(c => new UIGlobalMapRootViewModel()).AsSingle();
        }
    }
}