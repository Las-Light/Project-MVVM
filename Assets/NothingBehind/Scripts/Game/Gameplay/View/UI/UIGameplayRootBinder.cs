using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.MVVM.UI;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.UI
{
    public class UIGameplayRootBinder : UIRootBinder
    {
        // Если захотим что-то свое, то оверайдим OnBind
        
        // protected override void OnBind(UIRootViewModel uiRootViewModel)
        // {
        // }
    }
    // public class UIGameplayRootBinder : MonoBehaviour
    // {
    //     [SerializeField] private MapId targetMapId = MapId.Map_1;
    //     private Subject<GameplayExitParams> _exitSceneSignalSubj;
    //     
    //     public void HandleGoToMainMenuButtonClick()
    //     {
    //         _exitSceneSignalSubj?.OnNext(new GameplayExitParams(new SceneEnterParams(targetMapId)));
    //     }
    //
    //     public void Bind(Subject<GameplayExitParams> exitSceneSignalSubj)
    //     {
    //         _exitSceneSignalSubj = exitSceneSignalSubj;
    //     }
    // }
}
