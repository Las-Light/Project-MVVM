using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Transfers;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.Player;
using NothingBehind.Scripts.Game.GlobalMap.Root;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GlobalMap.MVVM.Maps
{
    public class GlobalMapTransferView: MonoBehaviour
    {
        private bool _triggered;
        private Subject<GlobalMapExitParams> _exitSceneSignalSubj;
        private MapTransferViewModel _viewModel;

        public void Bind(Subject<GlobalMapExitParams> exitSceneSignal, MapTransferViewModel viewModel)
        {
            _exitSceneSignalSubj = exitSceneSignal;
            _viewModel = viewModel;

            transform.position = viewModel.Position;
        }

        private void OnTriggerStay(Collider other)
        {
            if (_triggered)
                return;
            other.TryGetComponent<GMPlayerView>(out var playerView);
            if (playerView!=null)
            {
                if (playerView.IsInteractiveActionPressed())
                {
                    _exitSceneSignalSubj?.OnNext(
                        new GlobalMapExitParams(
                            new SceneEnterParams(
                                _viewModel.MapId)));
                    _triggered = true;
                }
            }
        }
    }
}