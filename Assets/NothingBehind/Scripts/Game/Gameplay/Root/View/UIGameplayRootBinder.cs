using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Maps;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public class UIGameplayRootBinder : MonoBehaviour
    {
        [SerializeField] private MapId targetMapId = MapId.Map_1;
        private Subject<GameplayExitParams> _exitSceneSignalSubj;
        
        public void HandleGoToMainMenuButtonClick()
        {
            _exitSceneSignalSubj?.OnNext(new GameplayExitParams(new SceneEnterParams(targetMapId)));
        }

        public void Bind(Subject<GameplayExitParams> exitSceneSignalSubj)
        {
            _exitSceneSignalSubj = exitSceneSignalSubj;
        }
    }
}
