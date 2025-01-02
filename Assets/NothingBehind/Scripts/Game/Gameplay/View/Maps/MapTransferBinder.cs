using NothingBehind.Scripts.Game.Gameplay.Root;
using NothingBehind.Scripts.Game.GameRoot;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Maps
{
    public class MapTransferBinder : MonoBehaviour
    {
        private bool _triggered;
        private Subject<GameplayExitParams> _exitSceneSignalSubj;
        private MapTransferViewModel _viewModel;

        public void Bind(Subject<GameplayExitParams> exitSceneSignal, MapTransferViewModel viewModel)
        {
            _exitSceneSignalSubj = exitSceneSignal;
            _viewModel = viewModel;

            transform.position = viewModel.Position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered)
                return;
            _exitSceneSignalSubj?.OnNext(
                new GameplayExitParams(
                    new SceneEnterParams(
                        _viewModel.MapId)));
            _triggered = true;
        }
    }
}