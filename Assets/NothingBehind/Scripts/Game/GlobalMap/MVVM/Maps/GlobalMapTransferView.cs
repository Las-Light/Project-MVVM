using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Player;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Player;
using NothingBehind.Scripts.Game.GlobalMap.Root;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GlobalMap.MVVM.Maps
{
    public class GlobalMapTransferView: MonoBehaviour
    {
        private bool _triggered;
        private Subject<GlobalMapExitParams> _exitSceneSignalSubj;
        private GlobalMapTransferViewModel _viewModel;

        public void Bind(Subject<GlobalMapExitParams> exitSceneSignal, GlobalMapTransferViewModel viewModel)
        {
            _exitSceneSignalSubj = exitSceneSignal;
            _viewModel = viewModel;

            transform.position = viewModel.Position;
        }

        private void OnTriggerStay(Collider other)
        {
            if (_triggered)
                return;
            other.TryGetComponent<PlayerView>(out var heroView);
            if (heroView!=null)
            {
                if (heroView.IsInteractiveActionPressed())
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