using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Root;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.Gameplay.View.UI.PopupA;
using NothingBehind.Scripts.Game.Gameplay.View.UI.PopupB;
using NothingBehind.Scripts.Game.Gameplay.View.UI.ScreenGameplay;
using NothingBehind.Scripts.MVVM.UI;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.View.UI
{
    public class GameplayUIManager : UIManager
    {
        private readonly Subject<GameplayExitParams> _exitSceneRequest;
        private readonly InventoryService _inventoryService;

        public GameplayUIManager(DIContainer container) : base(container)
        {
            _exitSceneRequest = container.Resolve<Subject<GameplayExitParams>>(AppConstants.EXIT_SCENE_REQUEST_TAG);
            _inventoryService = container.Resolve<InventoryService>();
        }

        public ScreenGameplayViewModel OpenScreenGameplay()
        {
            var viewModel = new ScreenGameplayViewModel(this, _exitSceneRequest);
            var rootUI = Container.Resolve<UIGameplayRootViewModel>();
            
            rootUI.OpenSreen(viewModel);

            return viewModel;
        }

        public InventoryViewModel OpenInventory(int ownerId)
        {
            var inventoryData = _inventoryService.GetInventoryDataProxy(ownerId);
            if (inventoryData != null)
            {
                if (!_inventoryService.InventoryMap.TryGetValue(ownerId, out var viewModel))
                {
                    viewModel = _inventoryService.CreateInventoryViewModel(inventoryData);
                }
                var rootUI = Container.Resolve<UIGameplayRootViewModel>();
                
                rootUI.OpenPopup(viewModel);
                return viewModel;
            }

            return null;
        }

        public PopupAViewModel OpenPopupA()
        {
            var a = new PopupAViewModel();
            var rootUI = Container.Resolve<UIGameplayRootViewModel>();
            
            rootUI.OpenPopup(a);

            return a;
        }
        
        public PopupBViewModel OpenPopupB()
        {
            var b = new PopupBViewModel();
            var rootUI = Container.Resolve<UIGameplayRootViewModel>();
            
            rootUI.OpenPopup(b);

            return b;
        }
    }
}