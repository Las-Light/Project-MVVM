using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons;
using R3;
using Unity.VisualScripting;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Characters
{
    public class CharacterView : MonoBehaviour
    {
        [SerializeField] private GameObject _arsenalPrefab;
        [SerializeField] private Transform _pistolParent;
        [SerializeField] private Transform _rifleParent;
        [SerializeField] private Transform _unarmedParent;
        public Transform pointToCheckClip;
        public LayerMask obstacleMask;
        
        public ArsenalView ArsenalView { get; private set; }
        
        private bool _triggered;
        private GameplayUIManager _gameplayUIManager;
        private int _characterId;

        private CompositeDisposable _disposables = new ();
        private CharacterViewModel _viewModel;

        public void Bind(CharacterViewModel viewModel, GameplayUIManager gameplayUIManager)
        {
            _viewModel = viewModel;
            transform.position = viewModel.Position.CurrentValue;
            _gameplayUIManager = gameplayUIManager;
            _characterId = viewModel.CharacterEntityId;
            ArsenalView = CreateArsenalView(viewModel.ArsenalViewModel);
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
                    _gameplayUIManager.OpenInventory(_viewModel.EntityType,
                        _characterId, 
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

        public ArsenalView CreateArsenalView(ArsenalViewModel arsenalViewModel)
        {
            var arsenal = Instantiate(_arsenalPrefab, transform);
            var arsenalView = arsenal.GetComponent<ArsenalView>();
            arsenalView.Bind(arsenalViewModel,
                _pistolParent,
                _rifleParent,
                _unarmedParent,
                pointToCheckClip,
                obstacleMask);
            return arsenalView;
        }
    }
}