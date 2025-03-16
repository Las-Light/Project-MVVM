using NothingBehind.Scripts.Game.Gameplay.View.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerView : MonoBehaviour
    {
        private PlayerViewModel _viewModel;

        public void Bind(PlayerViewModel viewModel, GameplayUIManager gameplayUIManager)
        {
            _viewModel = viewModel; 
            var currentPosOnMap = viewModel.CurrentMap;
            transform.position = currentPosOnMap.CurrentValue.Position.Value;
            var mainCamera = Camera.main;
            viewModel.SetHeroViewWithComponent(this, mainCamera);
        }

        private void Update()
        {
            _viewModel.Move();
            _viewModel.Look();
        }
        
        //этот метод для RootMotion, без него игрок не движется
        private void OnAnimatorMove()
        {
        }

        public bool IsInteractiveActionPressed()
        {
            return _viewModel.InteractiveActionPressed();
        }
    }
}