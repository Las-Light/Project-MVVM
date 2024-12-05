using NothingBehind.Scripts.Game.GameRoot;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.MainMenu.Root.View
{
    public class UIMainMenuRootBinder : MonoBehaviour
    {
        [SerializeField] private string targetMapId = Scenes.GAMEPLAY;
        private Subject<MainMenuExitParams> _exitSceneSignalSubj;

        public void HandleGoToGameplayButtonClick()
        {
            _exitSceneSignalSubj?.OnNext(new MainMenuExitParams(new SceneEnterParams(targetMapId)));
        }

        public void Bind(Subject<MainMenuExitParams> exitSceneSignalSubj)
        {
            _exitSceneSignalSubj = exitSceneSignalSubj;
        }
    }
}