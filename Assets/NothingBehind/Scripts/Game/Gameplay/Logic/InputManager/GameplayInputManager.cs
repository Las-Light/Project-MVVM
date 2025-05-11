using System;
using InputController;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.InputManager
{
    public class GameplayInputManager : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsReload => _isReload;
        public ReadOnlyReactiveProperty<bool> IsSwitchSlot1 => _isSwitchSlot1;
        public ReadOnlyReactiveProperty<bool> IsSwitchSlot2 => _isSwitchSlot2;

        public ReadOnlyReactiveProperty<Vector2> Move => _move;
        public ReadOnlyReactiveProperty<bool> IsInteract => _isInteract;
        public ReadOnlyReactiveProperty<bool> IsAttack => _isAttack;
        public ReadOnlyReactiveProperty<bool> IsSprint => _isSprint;
        public ReadOnlyReactiveProperty<bool> IsCrouch => _isCrouch;
        public ReadOnlyReactiveProperty<bool> IsAim => _isAim;
        public ReadOnlyReactiveProperty<bool> IsRotateCameraLeft => _isRotateCameraLeft;
        public ReadOnlyReactiveProperty<bool> IsRotateCameraRight => _isRotateCameraRight;
        public ReadOnlyReactiveProperty<Vector2> LookGamepad => _lookGamepad;
        public ReadOnlyReactiveProperty<Vector2> LookMouse => _lookMouse;
        public bool MouseIsActive => _inputController.Player.Look.WasPerformedThisFrame();
        
        public ReadOnlyReactiveProperty<bool> IsSubmit => _isSubmit;
        public ReadOnlyReactiveProperty<bool> IsCancel => _isCancel;
        public ReadOnlyReactiveProperty<Vector2> Navigation => _navigation;
        
        private readonly ReactiveProperty<bool> _isInteract = new();
        private readonly ReactiveProperty<bool> _isReload = new();
        private readonly ReactiveProperty<bool> _isSwitchSlot1 = new();
        private readonly ReactiveProperty<bool> _isSwitchSlot2 = new();
        private readonly ReactiveProperty<bool> _isAttack = new();
        private readonly ReactiveProperty<bool> _isSprint = new();
        private readonly ReactiveProperty<bool> _isAim = new();
        private readonly ReactiveProperty<bool> _isCrouch = new();
        private readonly ReactiveProperty<bool> _isRotateCameraLeft = new();
        private readonly ReactiveProperty<bool> _isRotateCameraRight = new();
        private readonly ReactiveProperty<Vector2> _move = new();
        private readonly ReactiveProperty<Vector2> _lookGamepad = new();
        private readonly ReactiveProperty<Vector2> _lookMouse = new();
        
        private readonly ReactiveProperty<bool> _isSubmit = new();
        private readonly ReactiveProperty<bool> _isCancel = new();
        private readonly ReactiveProperty<Vector2> _navigation = new();

        private readonly CompositeDisposable _disposables = new();


        private readonly InputControl _inputController;
        private PlayerGameplayInput _playerGameplayInput;
        private UIGameplayInput _uiGameplayInput;

        public GameplayInputManager()
        {
            // Возможно его надо создавать на уровне GameRoot в GameEntryPoint
            _inputController = new InputControl();
            _inputController.Enable();

            InitPlayerInput(_inputController);
            InitUIInput(_inputController);
            AddDisposables();
        }

        private void InitPlayerInput(InputControl inputController)
        {
            _playerGameplayInput = new PlayerGameplayInput(inputController);
            PlayerInputEnabled();

            _playerGameplayInput.LookGamepadInputReceived += OnLookGamepadInputReceived;
            _playerGameplayInput.LookMouseInputReceived += OnLookMouseInputReceived;
            _playerGameplayInput.MoveInputReceived += OnMoveInputReceived;
            _playerGameplayInput.AimInputReceived += OnAimInputReceived;
            _playerGameplayInput.CameraRotateRightInputReceived += OnCameraRotateRightInputReceived;
            _playerGameplayInput.CameraRotateLeftInputReceived += OnCameraRotateLeftInputReceived;
            _playerGameplayInput.CrouchInputReceived += OnCrouchInputReceived;
            _playerGameplayInput.InteractInputReceived += OnInteractInputReceived;
            _playerGameplayInput.AttackInputReceived += OnAttackInputReceived;
            _playerGameplayInput.ReloadInputReceived += OnReloadInputReceived;
            _playerGameplayInput.SprintInputReceived += OnSprintInputReceived;
            _playerGameplayInput.SwitchWeaponSlot2InputReceived += OnSwitchWeaponSlot2InputReceived;
            _playerGameplayInput.SwitchWeaponSlot1InputReceived += OnSwitchWeaponSlot1InputReceived;
        }

        private void InitUIInput(InputControl inputController)
        {
            _uiGameplayInput = new UIGameplayInput(inputController);
            UIInputEnabled();

            _uiGameplayInput.SubmitInputReceived += OnSubmitInputReceived;
            _uiGameplayInput.CancelInputReceived += OnCancelInputReceived;
            _uiGameplayInput.NavigationInputReceived += OnNavigationInputReceived;
        }

        private void OnCancelInputReceived(bool pressed)
        {
            _isCancel.OnNext(pressed);
        }
        
        private void OnNavigationInputReceived(Vector2 direction)
        {
            _navigation.OnNext(direction);
        }

        private void OnSubmitInputReceived(bool pressed)
        {
            _isSubmit.OnNext(pressed);
        }

        public void PlayerInputEnabled()
        {
            _inputController.Player.Enable();
        }
        public void UIInputEnabled()
        {
            _inputController.UI.Enable();
        }
        public void PlayerInputDisabled()
        {
            _inputController.Player.Disable();
        }
        public void UIInputDisabled()
        {
            _inputController.UI.Disable();
        }

        private void OnAimInputReceived(bool pressed)
        {
            _isAim.OnNext(pressed);
        }

        private void OnSwitchWeaponSlot2InputReceived(bool pressed)
        {
            _isSwitchSlot2?.OnNext(pressed);
        }
        
        private void OnSwitchWeaponSlot1InputReceived(bool pressed)
        {
            _isSwitchSlot1?.OnNext(pressed);
        }

        private void OnReloadInputReceived(bool pressed)
        {
            _isReload?.OnNext(pressed);
        }

        private void OnCrouchInputReceived(bool pressed)
        {
            _isCrouch.OnNext(pressed);
        }

        private void OnCameraRotateRightInputReceived(bool pressed)
        {
            _isRotateCameraRight.OnNext(pressed);
        }

        private void OnCameraRotateLeftInputReceived(bool pressed)
        {
            _isRotateCameraLeft.OnNext(pressed);
        }

        private void OnSprintInputReceived(bool pressed)
        {
            _isSprint.OnNext(pressed);
        }

        private void OnAttackInputReceived(bool pressed)
        {
            _isAttack.OnNext(pressed);
        }

        private void OnInteractInputReceived(bool pressed)
        {
            _isInteract.OnNext(pressed);
        }

        private void OnLookMouseInputReceived(Vector2 position)
        {
            _lookMouse.OnNext(position);
        }

        private void OnLookGamepadInputReceived(Vector2 direction)
        {
            _lookGamepad.OnNext(direction);
        }

        private void OnMoveInputReceived(Vector2 movementDirection)
        {
            _move.OnNext(movementDirection);
        }

        private void AddDisposables()
        {
            _disposables.Add(_isInteract);
            _disposables.Add(_isAttack);
            _disposables.Add(_isSprint);
            _disposables.Add(_isAim);
            _disposables.Add(_isRotateCameraLeft);
            _disposables.Add(_isRotateCameraRight);
            _disposables.Add(_move);
            _disposables.Add(_lookGamepad);
            _disposables.Add(_lookMouse);
            _disposables.Add(_isReload);
            _disposables.Add(_isSwitchSlot1);
            _disposables.Add(_isSwitchSlot2);
            _disposables.Add(_isSubmit);
            _disposables.Add(_isCancel);
            _disposables.Add(_navigation);
        }

        public void Dispose()
        {
            _playerGameplayInput.LookGamepadInputReceived -= OnLookGamepadInputReceived;
            _playerGameplayInput.LookMouseInputReceived -= OnLookMouseInputReceived;
            _playerGameplayInput.MoveInputReceived -= OnMoveInputReceived;
            _playerGameplayInput.AimInputReceived -= OnAimInputReceived;
            _playerGameplayInput.CameraRotateRightInputReceived -= OnCameraRotateRightInputReceived;
            _playerGameplayInput.CameraRotateLeftInputReceived -= OnCameraRotateLeftInputReceived;
            _playerGameplayInput.CrouchInputReceived -= OnCrouchInputReceived;
            _playerGameplayInput.InteractInputReceived -= OnInteractInputReceived;
            _playerGameplayInput.AttackInputReceived -= OnAttackInputReceived;
            _playerGameplayInput.ReloadInputReceived -= OnReloadInputReceived;
            _playerGameplayInput.SprintInputReceived -= OnSprintInputReceived;
            _playerGameplayInput.SwitchWeaponSlot2InputReceived -= OnSwitchWeaponSlot2InputReceived;
            _playerGameplayInput.SwitchWeaponSlot1InputReceived -= OnSwitchWeaponSlot1InputReceived;
            _uiGameplayInput.SubmitInputReceived -= OnSubmitInputReceived;
            _uiGameplayInput.CancelInputReceived -= OnCancelInputReceived;
            _uiGameplayInput.NavigationInputReceived -= OnNavigationInputReceived;
            _inputController.Disable();
            _disposables.Dispose();
        }
    }
}