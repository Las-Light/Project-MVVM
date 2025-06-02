using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Gameplay.Root;
using NothingBehind.Scripts.Game.GameRoot;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Maps
{
    public class GameplayMapTransferView : MonoBehaviour
    {
        private bool _triggered;
        private Subject<GameplayExitParams> _exitSceneSignalSubj;
        private GameplayMapTransferViewModel _viewModel;

        public void Bind(Subject<GameplayExitParams> exitSceneSignal, GameplayMapTransferViewModel viewModel)
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
                        new GameplayExitParams(
                            new SceneEnterParams(
                                _viewModel.MapId)));
                    _triggered = true;
                }
            }
        }
    }
}