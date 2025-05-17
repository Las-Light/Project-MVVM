using NothingBehind.Scripts.Game.Gameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Storages
{
    public class StorageView : MonoBehaviour
    {
        public bool IsEmpty { get; set; }
        
        private StorageViewModel _storageViewModel;
        private int _storageId;
        private bool _triggered;
        private GameplayUIManager _gameplayUIManager;
        
        private readonly CompositeDisposable _disposables = new();

        public void Bind(StorageViewModel storageViewModel, 
            GameplayUIManager gameplayUIManager)
        {
            transform.position = storageViewModel.Position.CurrentValue;
            _storageViewModel = storageViewModel;
            _gameplayUIManager = gameplayUIManager;
            _storageId = storageViewModel.Id;
            IsEmpty = storageViewModel.IsEmptyInventory();
        }


        private void OnDestroy()
        {
            _disposables.Dispose();
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
                    _gameplayUIManager.OpenInventory(_storageViewModel.EntityType,
                        _storageId, 
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