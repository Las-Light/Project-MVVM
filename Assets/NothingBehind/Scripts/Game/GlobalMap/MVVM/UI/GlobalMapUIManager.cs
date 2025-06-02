using DI.Scripts;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.UI.ScreenGlobalMap;
using NothingBehind.Scripts.MVVM.UI;

namespace NothingBehind.Scripts.Game.GlobalMap.MVVM.UI
{
    public class GlobalMapUIManager : UIManager
    {
        public GlobalMapUIManager(DIContainer container) : base(container)
        {
            
        }
        
        public ScreenGlobalMapViewModel OpenScreenGameplay()
        {
            var viewModel = new ScreenGlobalMapViewModel();
            var rootUI = Container.Resolve<UIGlobalMapRootViewModel>();

            rootUI.OpenSreen(viewModel);

            return viewModel;
        }
    }
}