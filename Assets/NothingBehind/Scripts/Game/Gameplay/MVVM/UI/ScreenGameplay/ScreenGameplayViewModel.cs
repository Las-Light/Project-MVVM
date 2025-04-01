using NothingBehind.Scripts.Game.Gameplay.Root;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.MVVM.UI;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.UI.ScreenGameplay
{
    public class ScreenGameplayViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private readonly Subject<GameplayExitParams> _exitSceneRequest;
        public override string Id => "ScreenGameplay";

        public ScreenGameplayViewModel(GameplayUIManager uiManager, Subject<GameplayExitParams> exitSceneRequest)
        {
            _uiManager = uiManager;
            _exitSceneRequest = exitSceneRequest;
        }

        public void RequestOpenInventory(int ownerId)
        {
            _uiManager.OpenInventory(ownerId);
        }

        public void RequestOpenPopupA()
        {
            _uiManager.OpenPopupA();
        }

        public void RequestOpenPopupB()
        {
            _uiManager.OpenPopupB();
        }

        public void RequestGoToMainMenu()
        {
            // здесь руками указываю, что переход осуществляется на MapId.MainMenu 
            _exitSceneRequest.OnNext(new GameplayExitParams(new SceneEnterParams(MapId.MainMenu))); 
        }
    }
}