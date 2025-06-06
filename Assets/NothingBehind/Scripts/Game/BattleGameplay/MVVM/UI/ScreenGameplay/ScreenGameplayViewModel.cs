using System;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.BattleGameplay.Root;
using NothingBehind.Scripts.Game.BattleGameplay.Services;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.MVVM.UI;
using R3;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.UI.ScreenGameplay
{
    public class ScreenGameplayViewModel : WindowViewModel
    {
        public readonly ArsenalViewModel ArsenalViewModel;
        
        private readonly GameplayUIManager _uiManager;
        private readonly Subject<GameplayExitParams> _exitSceneRequest;
        public override string Id => "ScreenGameplay";

        public ScreenGameplayViewModel(GameplayUIManager uiManager, 
            Subject<GameplayExitParams> exitSceneRequest,
            PlayerService playerService,
            ArsenalService arsenalService)
        {
            _uiManager = uiManager;
            _exitSceneRequest = exitSceneRequest;
            if (arsenalService.ArsenalMap.TryGetValue(playerService.PlayerViewModel.Value.Id, out var arsenalViewModel))
            {
                ArsenalViewModel = arsenalViewModel;
            }
            else
            {
                throw new Exception(
                    $"ArsenalViewModel for owner with Id {playerService.PlayerViewModel.Value.Id} not found");
            }
        }

        public void RequestOpenInventory(int ownerId)
        {
            //_uiManager.OpenInventory(ownerId);
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