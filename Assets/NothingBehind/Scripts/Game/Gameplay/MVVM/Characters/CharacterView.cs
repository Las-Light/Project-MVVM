using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Characters
{
    public class CharacterView : MonoBehaviour
    {
        private bool _triggered;
        private GameplayUIManager _gameplayUIManager;
        private int _characterId;

        private CompositeDisposable _disposables = new ();
        
        public void Bind(CharacterViewModel viewModel, GameplayUIManager gameplayUIManager)
        {
            transform.position = viewModel.Position.CurrentValue;
            _gameplayUIManager = gameplayUIManager;
            _characterId = viewModel.CharacterEntityId;
            
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
                    _gameplayUIManager.OpenInventory(_characterId, 
                        playerView.PlayerId,
                        playerView.transform.position);
                    _triggered = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _triggered = false;
        }
    }
}