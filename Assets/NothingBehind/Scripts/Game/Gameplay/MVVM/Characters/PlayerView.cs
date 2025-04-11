using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Characters
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private GameObject _arsenalPrefab;
        [SerializeField] private Transform _pistolParent;
        [SerializeField] private Transform _rifleParent;
        [SerializeField] private Transform _unarmedParent;
        private PlayerViewModel _viewModel;
        private ArsenalView _arsenalView;
        private GameplayUIManager _gameplayUIManager;
        private bool _inventoruIsOpened;

        public void Bind(PlayerViewModel viewModel, GameplayUIManager gameplayUIManager)
        {
            _viewModel = viewModel;
            var currentMap = viewModel.CurrentMapId.CurrentValue;
            var currentPosOnMap = viewModel.PositionOnMaps.First(posOnMap => posOnMap.MapId == currentMap);
            transform.position = currentPosOnMap.Position.Value;
            var mainCamera = Camera.main;
            viewModel.SetPlayerViewWithComponent(this, mainCamera);
            _arsenalView = CreateArsenalView(viewModel.ArsenalViewModel);
            _gameplayUIManager = gameplayUIManager;
        }

        private void Update()
        {
            _viewModel.Move();
            _viewModel.Look();
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _arsenalView.WeaponSwitch(_arsenalView.WeaponSlot1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _arsenalView.WeaponSwitch(_arsenalView.WeaponSlot2);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (!_inventoruIsOpened)
                {
                    _gameplayUIManager.OpenInventory(_viewModel.Id);
                    _inventoruIsOpened = true;
                }
                else
                {
                    _gameplayUIManager.CloseInventory();
                    _inventoruIsOpened = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _arsenalView.Reload();
            }
        }
        
        //этот метод для RootMotion, без него игрок не движется
        private void OnAnimatorMove()
        {
        }

        public bool IsInteractiveActionPressed()
        {
            return _viewModel.InteractiveActionPressed();
        }

        private ArsenalView CreateArsenalView(ArsenalViewModel arsenalViewModel)
        {
            var arsenal = Instantiate(_arsenalPrefab, transform);
            var arsenalView = arsenal.GetComponent<ArsenalView>();
            arsenalView.Bind(arsenalViewModel, _pistolParent, _rifleParent, _unarmedParent);
            return arsenalView;
        }
    }
}