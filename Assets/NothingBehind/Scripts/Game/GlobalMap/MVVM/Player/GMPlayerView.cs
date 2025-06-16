using NothingBehind.Scripts.Game.GameRoot.MVVM.Inventories;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Player;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.GlobalMap.Logic.ActionController;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.UI;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GlobalMap.MVVM.Player
{
    public class GMPlayerView : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        private GMPlayerMovementController _movementController;
        private PlayerViewModel _viewModel;
        private UnityEngine.Camera _mainCamera;
        private InputManager _inputManager;

        public void Bind(PlayerViewModel viewModel,
            InventoryViewModel inventoryViewModel, 
            GlobalMapUIManager gameplayUIManager)
        {
            _viewModel = viewModel;
            _inputManager = viewModel.InputManager;
            _movementController = GetComponent<GMPlayerMovementController>();
            _mainCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            if (_inputManager.IsAttack.CurrentValue)
            {
                var ray = _mainCamera.ScreenPointToRay(_inputManager.LookMouse.CurrentValue);
                if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, _layerMask))
                {
                    _movementController.MoveToTarget(raycastHit.point);
                }
            }
        }
        
        //этот метод для RootMotion, без него игрок не движется
        private void OnAnimatorMove()
        {
        }
        
        public bool IsInteractiveActionPressed()
        {
            return _inputManager.IsInteract.CurrentValue;
        }
    }
}