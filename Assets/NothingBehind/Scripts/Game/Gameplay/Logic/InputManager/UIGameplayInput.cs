using System;
using UnityEngine;
using UnityEngine.InputSystem;
using InputControl = InputController.InputControl;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.InputManager
{
    public class UIGameplayInput : IDisposable
    {
        public event Action<bool> SubmitInputReceived;
        public event Action<bool> CancelInputReceived;
        public event Action<Vector2> NavigationInputReceived;
        
        private readonly InputControl _inputController;

        public UIGameplayInput(InputControl inputController)
        {
            _inputController = inputController;

            _inputController.UI.Submit.performed += OnSubmitPerformed;
            _inputController.UI.Submit.canceled += OnSubmitPerformed;
            _inputController.UI.Cancel.performed += OnCancelPerformed;
            _inputController.UI.Cancel.canceled += OnCancelPerformed;
            _inputController.UI.Navigate.performed += OnNavigationPerformed;
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            CancelInputReceived?.Invoke(context.ReadValueAsButton());
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context)
        {
            SubmitInputReceived?.Invoke(context.ReadValueAsButton());
        }
        private void OnNavigationPerformed(InputAction.CallbackContext context)
        {
            NavigationInputReceived?.Invoke(context.ReadValue<Vector2>());
        }


        public void Dispose()
        {
            _inputController.UI.Submit.performed -= OnSubmitPerformed;
            _inputController.UI.Cancel.performed -= OnCancelPerformed;
            _inputController.UI.Navigate.performed -= OnNavigationPerformed;
            
            _inputController.UI.Disable();
        }
    }
}