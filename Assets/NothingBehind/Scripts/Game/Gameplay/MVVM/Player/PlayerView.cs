using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private GameObject _arsenalPrefab;
        [SerializeField] private Transform _pistolParent;
        [SerializeField] private Transform _rifleParent;
        [SerializeField] private Transform _unarmedParent;
        
        public PlayerSettings PlayerSettings { get; private set; }
        public GameplayInputManager InputManager { get; private set; }
        
        private PlayerViewModel _viewModel;
        private TurnController _turnController;
        private MovementController _movementController;
        private ArsenalView _arsenalView;
        private GameplayUIManager _gameplayUIManager;
        private bool _inventoryIsOpened;

        public void Bind(PlayerViewModel viewModel, GameplayUIManager gameplayUIManager)
        {
            _viewModel = viewModel;
            var currentMap = viewModel.CurrentMapId.CurrentValue;
            var currentPosOnMap = viewModel.PositionOnMaps.First(posOnMap => posOnMap.MapId == currentMap);
            transform.position = currentPosOnMap.Position.Value;
            _gameplayUIManager = gameplayUIManager;
            InputManager = viewModel.InputManager;
            PlayerSettings = viewModel.PlayerSettings;
            _turnController = GetComponent<TurnController>();
            _movementController = GetComponent<MovementController>();
            
            _arsenalView = CreateArsenalView(viewModel.ArsenalViewModel);
        }

        private void Update()
        {
            _movementController.Move();
            _viewModel.UpdatePlayerPosition(transform.position);
            _turnController.Look();
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
                if (!_inventoryIsOpened)
                {
                    _gameplayUIManager.OpenInventory(_viewModel.Id);
                    _inventoryIsOpened = true;
                }
                else
                {
                    _gameplayUIManager.CloseInventory();
                    _inventoryIsOpened = false;
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
            return InputManager.IsInteract.CurrentValue;
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