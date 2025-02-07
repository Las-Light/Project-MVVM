using System;
using InputController;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.InputManager
{
    public class GameplayInputManager : IDisposable
    {
        public event Action ReloadInputReceived;
        public event Action SwitchWeaponInputReceived;

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
        
        private readonly ReactiveProperty<bool> _isInteract = new();
        private readonly ReactiveProperty<bool> _isAttack = new();
        private readonly ReactiveProperty<bool> _isSprint = new();
        private readonly ReactiveProperty<bool> _isAim = new();
        private readonly ReactiveProperty<bool> _isCrouch = new();
        private readonly ReactiveProperty<bool> _isRotateCameraLeft = new();
        private readonly ReactiveProperty<bool> _isRotateCameraRight = new();
        private readonly ReactiveProperty<Vector2> _move = new();
        private readonly ReactiveProperty<Vector2> _lookGamepad = new();
        private readonly ReactiveProperty<Vector2> _lookMouse = new();

        private readonly CompositeDisposable _disposables = new();


        private readonly InputControl _inputController;
        private PlayerGameplayInput _gameplayInput;

        public GameplayInputManager()
        {
            // Возможно его надо создавать на уровне GameRoot в GameEntryPoint
            _inputController = new InputControl();
            _inputController.Enable();

            InitPlayerInput(_inputController);
            AddDisposables();
        }

        private void InitPlayerInput(InputControl inputController)
        {
            _gameplayInput = new PlayerGameplayInput(inputController);

            _gameplayInput.LookGamepadInputReceived += OnLookGamepadInputReceived;
            _gameplayInput.LookMouseInputReceived += OnLookMouseInputReceived;
            _gameplayInput.MoveInputReceived += OnMoveInputReceived;
            _gameplayInput.AimInputReceived += OnAimInputReceived;
            _gameplayInput.CameraRotateRightInputReceived += OnCameraRotateRightInputReceived;
            _gameplayInput.CameraRotateLeftInputReceived += OnCameraRotateLeftInputReceived;
            _gameplayInput.CrouchInputReceived += OnCrouchInputReceived;
            _gameplayInput.InteractInputReceived += OnInteractInputReceived;
            _gameplayInput.AttackInputReceived += OnAttackInputReceived;
            _gameplayInput.ReloadInputReceived += OnReloadInputReceived;
            _gameplayInput.SprintInputReceived += OnSprintInputReceived;
            _gameplayInput.SwitchWeaponInputReceived += OnSwitchWeaponInputReceived;
        }

        private void OnAimInputReceived(bool pressed)
        {
            _isAim.OnNext(pressed);
        }

        private void OnSwitchWeaponInputReceived()
        {
            SwitchWeaponInputReceived?.Invoke();
        }

        private void OnReloadInputReceived()
        {
            ReloadInputReceived?.Invoke();
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
        }

        public void Dispose()
        {
            _gameplayInput.LookGamepadInputReceived -= OnLookGamepadInputReceived;
            _gameplayInput.LookMouseInputReceived -= OnLookMouseInputReceived;
            _gameplayInput.MoveInputReceived -= OnMoveInputReceived;
            _gameplayInput.AimInputReceived -= OnAimInputReceived;
            _gameplayInput.CameraRotateRightInputReceived -= OnCameraRotateRightInputReceived;
            _gameplayInput.CameraRotateLeftInputReceived -= OnCameraRotateLeftInputReceived;
            _gameplayInput.CrouchInputReceived -= OnCrouchInputReceived;
            _gameplayInput.InteractInputReceived -= OnInteractInputReceived;
            _gameplayInput.AttackInputReceived -= OnAttackInputReceived;
            _gameplayInput.ReloadInputReceived -= OnReloadInputReceived;
            _gameplayInput.SprintInputReceived -= OnSprintInputReceived;
            _gameplayInput.SwitchWeaponInputReceived -= OnSwitchWeaponInputReceived;
            _inputController.Disable();
            _disposables.Dispose();
        }
    }
}