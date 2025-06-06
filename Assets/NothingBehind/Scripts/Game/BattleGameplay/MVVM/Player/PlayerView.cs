using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.UI;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Inventories;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Player;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Player
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private GameObject _arsenalPrefab;
        [SerializeField] private Transform _pistolParent;
        [SerializeField] private Transform _rifleParent;
        [SerializeField] private Transform _unarmedParent;
        [Header("Clip Prevention")] 
        [Tooltip("ViewPoint")]
        public Transform pointToCheckClip;
        public LayerMask obstacleMask;
        
        public PlayerSettings PlayerSettings { get; private set; }
        public InputManager InputManager { get; private set; }
        public ArsenalView ArsenalView { get; private set; }
        public int PlayerId { get; private set; }
        
        public GameObject CurrentEnemy { get; private set; }
        public bool IsAim;
        public bool IsCrouch;
        public bool IsSprint;
        public bool IsCheckWall;
        public bool IsReloading;

        public List<GameObject> VisibleTargets = new();
        
        private PlayerViewModel _viewModel;
        private LookPlayerController _lookPlayerController;
        private AimController _aimController;
        private PlayerMovementController _playerMovementController;
        private GameplayUIManager _gameplayUIManager;
        private bool _inventoryIsOpened;

        private CompositeDisposable _disposables = new();

        public void Bind(PlayerViewModel viewModel,
            ArsenalViewModel arsenalViewModel,
            InventoryViewModel inventoryViewModel,
            GameplayUIManager gameplayUIManager)
        {
            _viewModel = viewModel;
            PlayerId = viewModel.Id;
            var currentMap = viewModel.CurrentMapId.CurrentValue;
            var currentPosOnMap = viewModel.PositionOnMaps.First(posOnMap => posOnMap.MapId == currentMap);
            transform.position = currentPosOnMap.Position.Value;
            _gameplayUIManager = gameplayUIManager;
            InputManager = viewModel.InputManager;
            PlayerSettings = viewModel.PlayerSettings;
            _lookPlayerController = GetComponent<LookPlayerController>();
            _playerMovementController = GetComponent<PlayerMovementController>();
            _aimController = GetComponent<AimController>();
            
            ArsenalView = CreateArsenalView(arsenalViewModel);
            
            _disposables.Add(InputManager.IsAim.Skip(1).Subscribe(isAim =>
            {
                if (isAim)
                {
                    IsAim = true;
                    _aimController.Aim(ArsenalView, IsCheckWall);
                }
                else
                {
                    IsAim = false;
                    _aimController.RemoveAim(ArsenalView);
                }
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
            _playerMovementController.Move();
            _lookPlayerController.Look();
            //_viewModel.UpdatePlayerPosition(transform.position);
            PressShoot();
            if (ArsenalView.ActiveGun.WeaponType != WeaponType.Unarmed)
            {
                ArsenalView.ClipPrevention(IsAim, ref IsCheckWall);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (!_inventoryIsOpened)
                {
                    _gameplayUIManager.OpenInventory(_viewModel.EntityType,
                        _viewModel.Id,
                        _viewModel.Id,
                        transform.position);
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