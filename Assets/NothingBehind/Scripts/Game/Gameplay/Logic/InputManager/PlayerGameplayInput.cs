using System;
using UnityEngine;
using UnityEngine.InputSystem;
using InputControl = InputController.InputControl;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.InputManager
{
    public class PlayerGameplayInput : IDisposable
    {
        public event Action ReloadInputReceived;
        public event Action SwitchWeaponInputReceived;
        public event Action<bool> CameraRotateRightInputReceived;
        public event Action<bool> CameraRotateLeftInputReceived;
        public event Action<bool> InteractInputReceived;
        public event Action<bool> AttackInputReceived;
        public event Action<bool> AimInputReceived;
        public event Action<bool> SprintInputReceived;
        public event Action<bool> CrouchInputReceived;
        public event Action<Vector2> MoveInputReceived;
        public event Action<Vector2> LookMouseInputReceived;
        public event Action<Vector2> LookGamepadInputReceived;

        private readonly InputControl _inputController;

        public PlayerGameplayInput(InputControl inputController)
        {
            _inputController = inputController;

            _inputController.Player.Look.performed += OnLookMousePerformed;
            _inputController.Player.LookGamepad.performed += OnLookGamepadPerformed;
            _inputController.Player.Move.performed += OnMove;
            _inputController.Player.Move.canceled += OnMove;
            _inputController.Player.Interact.performed += OnInteract;
            _inputController.Player.Interact.canceled += OnInteract;
            _inputController.Player.Aim.performed += OnAimPerformed;
            _inputController.Player.RotateCameraRight.performed += OnRotateCameraRightPerformed;
            _inputController.Player.RotateCameraLeft.performed += OnRotateCameraLeftPerformed;
            _inputController.Player.Crouch.performed += OnCrouchPerformed;
            _inputController.Player.Attack.performed += OnAttackPerformed;
            _inputController.Player.Reload.performed += OnReloadPerformed;
            _inputController.Player.Sprint.performed += OnSprintPerformed;
            _inputController.Player.SwitchWeapon.performed += OnSwitchWeaponPerformed;
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            InteractInputReceived?.Invoke(context.ReadValueAsButton());
        }

        private void OnAimPerformed(InputAction.CallbackContext context)
        {
            AimInputReceived?.Invoke(context.ReadValueAsButton());
        }

        private void OnSwitchWeaponPerformed(InputAction.CallbackContext context)
        {
            SwitchWeaponInputReceived?.Invoke();
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            SprintInputReceived?.Invoke(context.ReadValueAsButton());
        }

        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            ReloadInputReceived?.Invoke();
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AttackInputReceived?.Invoke(context.ReadValueAsButton());
        }

        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            CrouchInputReceived?.Invoke(context.ReadValueAsButton());
        }

        private void OnLookMousePerformed(InputAction.CallbackContext context)
        {
            LookMouseInputReceived?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnLookGamepadPerformed(InputAction.CallbackContext context)
        {
            LookGamepadInputReceived?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnRotateCameraRightPerformed(InputAction.CallbackContext context)
        {
            CameraRotateRightInputReceived?.Invoke(context.ReadValueAsButton());
        }

        private void OnRotateCameraLeftPerformed(InputAction.CallbackContext context)
        {
            CameraRotateLeftInputReceived?.Invoke(context.ReadValueAsButton());
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            MoveInputReceived?.Invoke(context.ReadValue<Vector2>());
        }

        public void Dispose()
        {
            _inputController.Player.Look.performed -= OnLookMousePerformed;
            _inputController.Player.LookGamepad.performed -= OnLookGamepadPerformed;
            _inputController.Player.Move.performed -= OnMove;
            _inputController.Player.Move.canceled -= OnMove;
            _inputController.Player.Interact.performed -= OnInteract;
            _inputController.Player.Interact.canceled -= OnInteract;
            _inputController.Player.Aim.performed -= OnAimPerformed;
            _inputController.Player.RotateCameraRight.performed -= OnRotateCameraRightPerformed;
            _inputController.Player.RotateCameraLeft.performed -= OnRotateCameraLeftPerformed;
            _inputController.Player.Crouch.performed -= OnCrouchPerformed;
            _inputController.Player.Attack.performed -= OnAttackPerformed;
            _inputController.Player.Reload.performed -= OnReloadPerformed;
            _inputController.Player.Sprint.performed -= OnSprintPerformed;
            _inputController.Player.SwitchWeapon.performed -= OnSwitchWeaponPerformed;

            _inputController.Player.Disable();
        }
    }
}