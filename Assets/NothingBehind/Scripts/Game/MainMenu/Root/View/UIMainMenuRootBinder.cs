using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Maps;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.MainMenu.Root.View
{
    public class UIMainMenuRootBinder : MonoBehaviour
    {
        [SerializeField] private MapId targetMapId = MapId.Map_1;
        [SerializeField] private string targetSceneName = Scenes.GAMEPLAY;
        private Subject<MainMenuExitParams> _exitSceneSignalSubj;

        public void HandleGoToGameplayButtonClick()
        {
            _exitSceneSignalSubj?.OnNext(new MainMenuExitParams(new SceneEnterParams(targetSceneName, targetMapId.ToString())));
        }

        public void Bind(Subject<MainMenuExitParams> exitSceneSignalSubj)
        {
            _exitSceneSignalSubj = exitSceneSignalSubj;
        }
    }
}