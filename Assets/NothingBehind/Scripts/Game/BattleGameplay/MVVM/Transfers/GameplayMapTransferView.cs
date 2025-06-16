using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Player;
using NothingBehind.Scripts.Game.BattleGameplay.Root;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Transfers;
using NothingBehind.Scripts.Game.State;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Transfers
{
    public class GameplayMapTransferView : MonoBehaviour
    {
        private bool _triggered;
        private Subject<GameplayExitParams> _exitSceneSignalSubj;
        private MapTransferViewModel _viewModel;
        private IGameStateProvider _gameStateProvider;

        public void Bind(Subject<GameplayExitParams> exitSceneSignal, 
            IGameStateProvider gameStateProvider,
            MapTransferViewModel viewModel)
        {
            _gameStateProvider = gameStateProvider;
            _exitSceneSignalSubj = exitSceneSignal;
            _viewModel = viewModel;

            transform.position = viewModel.Position;
        }

        private void OnTriggerStay(Collider other)
        {
            if (_triggered)
                return;
            other.TryGetComponent<PlayerView>(out var playerView);
            if (playerView!=null)
            {
                if (playerView.IsInteractiveActionPressed())
                {
                    _gameStateProvider.SaveGameState();
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