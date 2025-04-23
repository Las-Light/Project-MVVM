using System;
using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Player
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private GameObject _arsenalPrefab;
        [SerializeField] private Transform _pistolParent;
        [SerializeField] private Transform _rifleParent;
        [SerializeField] private Transform _unarmedParent;
        [Header("Clip Prevention")] 
        [Tooltip("ViewPoint")][SerializeField]
        public Transform pointToCheckClip;
        public LayerMask obstacleMask;
        
        public PlayerSettings PlayerSettings { get; private set; }
        public GameplayInputManager InputManager { get; private set; }
        public ArsenalView ArsenalView { get; private set; }
        public int PlayerId { get; private set; }
        
        public GameObject CurrentEnemy { get; private set; }
        public bool IsAim;
        public bool IsCrouch;
        public bool IsSprint;
        public bool IsCheckWall;
        public bool IsReloading;

        public List<GameObject> VisibleTargets;
        
        private PlayerViewModel _viewModel;
        private TurnController _turnController;
        private AimController _aimController;
        private MovementController _movementController;
        private GameplayUIManager _gameplayUIManager;
        private bool _inventoryIsOpened;

        private CompositeDisposable _disposables = new();

        public void Bind(PlayerViewModel viewModel, GameplayUIManager gameplayUIManager)
        {
            _viewModel = viewModel;
            PlayerId = viewModel.Id;
            var currentMap = viewModel.CurrentMapId.CurrentValue;
            var currentPosOnMap = viewModel.PositionOnMaps.First(posOnMap => posOnMap.MapId == currentMap);
            transform.position = currentPosOnMap.Position.Value;
            _gameplayUIManager = gameplayUIManager;
            InputManager = viewModel.InputManager;
            PlayerSettings = viewModel.PlayerSettings;
            _turnController = GetComponent<TurnController>();
            _movementController = GetComponent<MovementController>();
            _aimController = GetComponent<AimController>();
            CurrentEnemy = viewModel.CurrentEnemy;
            IsAim = viewModel.IsAim;
            IsCrouch = viewModel.IsCrouch;
            IsSprint = viewModel.IsSprint;
            IsCheckWall = viewModel.IsCheckWall;
            IsReloading = viewModel.IsReloading;
            VisibleTargets = viewModel.VisibleTargets;
            
            ArsenalView = CreateArsenalView(viewModel.ArsenalViewModel);
            
            _disposables.Add(InputManager.IsAim.Skip(1).Subscribe(isAim =>
            {
                if (isAim)
                    _aimController.Aim();
                else
                    _aimController.RemoveAim();
            }));
            _disposables.Add(InputManager.IsReload.Skip(1).Subscribe(_ =>
            {
                ArsenalView.Reload();
            }));
            _disposables.Add(InputManager.IsSwitchSlot1.Skip(1).Subscribe(_ =>
            {
                if (!InputManager.IsAim.CurrentValue)
                {
                    ArsenalView.WeaponSwitch(ArsenalView.WeaponSlot1);
                }
            }));
            _disposables.Add(InputManager.IsSwitchSlot2.Skip(1).Subscribe(_ =>
            {
                if (!InputManager.IsAim.CurrentValue)
                {
                    ArsenalView.WeaponSwitch(ArsenalView.WeaponSlot2);
                }
            }));
        }

        private void Update()
        {
            _movementController.Move();
            _turnController.Look();
            //_viewModel.UpdatePlayerPosition(transform.position);
            PressShoot();
            if (ArsenalView.ActiveGun.WeaponType != WeaponType.Unarmed)
            {
                ArsenalView.ClipPrevention();
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (!_inventoryIsOpened)
                {
                    _gameplayUIManager.OpenInventory(_viewModel.Id, _viewModel.Id, transform.position);
                    _inventoryIsOpened = true;
                }
                else
                {
                    _gameplayUIManager.CloseInventory();
                    _inventoryIsOpened = false;
                }
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        //этот метод для RootMotion, без него игрок не движется
        private void OnAnimatorMove()
        {
        }
        
        public bool IsInteractiveActionPressed()
        {
            return InputManager.IsInteract.CurrentValue;
        }
        
        //метод производит выстрел при нажатии на кнопку "Стрелять" в режиме прицеливания
        private void PressShoot()
        {
            if (InputManager.IsAttack.CurrentValue && IsAim)
            {
                ArsenalView.Shoot();
            }
        }

        private ArsenalView CreateArsenalView(ArsenalViewModel arsenalViewModel)
        {
            var arsenal = Instantiate(_arsenalPrefab, transform);
            var arsenalView = arsenal.GetComponent<ArsenalView>();
            arsenalView.Bind(this, arsenalViewModel,
                _pistolParent, _rifleParent, _unarmedParent);
            return arsenalView;
        }
    }
}