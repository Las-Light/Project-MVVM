using System;
using UnityEngine;
using UnityEngine.InputSystem;
using InputControl = InputController.InputControl;

namespace NothingBehind.Scripts.Game.Gameplay.Services.InputManager
{
    public class PlayerGameplayInput
    {
        public event Action CrouchInputReceived;
        public event Action ReloadInputReceived;
        public event Action SwitchWeaponInputReceived;
        public event Action CameraRotateRightInputReceived;
        public event Action CameraRotateLeftInputReceived;
        public event Action<bool> ShootInputReceived;
        public event Action<bool> AimInputReceived;
        public event Action<bool> SprintInputReceived;
        public event Action<Vector2> MoveInputReceived;
        public event Action<Vector2> LookMouseInputReceived;
        public event Action<Vector2> LookGamepadInputReceived;

        private readonly InputControl _inputController;

        public PlayerGameplayInput(InputControl inputController)
        {
            _inputController = inputController;

            _inputController.Player.Look.performed += OnLookMousePerformed;
            _inputController.Player.LookGamepad.performed += OnLookGamepadPerformed;
            _inputController.Player.Move.performed += OnMovePerformed;
            _inputController.Player.Aim.performed += OnAimPerformed;
            _inputController.Player.RotateCameraRight.performed += OnRotateCameraRightPerformed;
            _inputController.Player.RotateCameraLeft.performed += OnRotateCameraLeftPerformed;
            _inputController.Player.Crouch.performed += OnCrouchPerformed;
            _inputController.Player.Attack.performed += OnShootPerformed;
            _inputController.Player.Reload.performed += OnReloadPerformed;
            _inputController.Player.Sprint.performed += OnSprintPerformed;
            _inputController.Player.SwitchWeapon.performed += OnSwitchWeaponPerformed;
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

        private void OnShootPerformed(InputAction.CallbackContext context)
        {
            ShootInputReceived?.Invoke(context.ReadValueAsButton());
        }

        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            CrouchInputReceived?.Invoke();
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
            CameraRotateRightInputReceived?.Invoke();
        }
        
        private void OnRotateCameraLeftPerformed(InputAction.CallbackContext context)
        {
            CameraRotateLeftInputReceived?.Invoke();
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            MoveInputReceived?.Invoke(context.ReadValue<Vector2>());
        }
    }
}