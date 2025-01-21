using System;
using InputController;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services.InputManager
{
    public class GameplayInputManager : IDisposable
    {
        public event Action<bool> AimInputReceived;
        public event Action RotationCameraRightInputReceived;
        public event Action RotationCameraLeftInputReceived;
        public event Action CrouchInputReceived;
        public event Action ReloadInputReceived;
        public event Action SwitchWeaponInputReceived;

        public ReadOnlyReactiveProperty<Vector2> Move => _move;
        public ReadOnlyReactiveProperty<bool> IsInteract => _isInteract;
        public ReadOnlyReactiveProperty<bool> IsAttack => _isAttack;
        public Vector2 LookGamepad { get; private set; }
        public Vector2 LookMouse { get; private set; }
        public bool MouseIsActive => _inputController.Player.Look.WasPerformedThisFrame();
        public bool IsSprint { get; private set; }
        
        private readonly ReactiveProperty<bool> _isInteract = new();
        private readonly ReactiveProperty<bool> _isAttack = new();

        private readonly ReactiveProperty<Vector2> _move = new();


        private readonly InputControl _inputController;
        private PlayerGameplayInput _gameplayInput;

        public GameplayInputManager()
        {
            // Возможно его надо создавать на уровне GameRoot в GameEntryPoint
            _inputController = new InputControl();
            _inputController.Enable();

            InitPlayerInput(_inputController);
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
            AimInputReceived?.Invoke(pressed);
        }

        private void OnSwitchWeaponInputReceived()
        {
            SwitchWeaponInputReceived?.Invoke();
        }

        private void OnReloadInputReceived()
        {
            ReloadInputReceived?.Invoke();
        }

        private void OnCrouchInputReceived()
        {
            CrouchInputReceived?.Invoke();
        }

        private void OnCameraRotateRightInputReceived()
        {
            RotationCameraRightInputReceived?.Invoke();
        }

        private void OnCameraRotateLeftInputReceived()
        {
            RotationCameraLeftInputReceived?.Invoke();
        }

        private void OnSprintInputReceived(bool pressed)
        {
            IsSprint = pressed;
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
            LookMouse = position;
        }

        private void OnLookGamepadInputReceived(Vector2 direction)
        {
            LookGamepad = direction;
        }

        private void OnMoveInputReceived(Vector2 movementDirection)
        {
            _move.OnNext(movementDirection);
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
        }
    }
}