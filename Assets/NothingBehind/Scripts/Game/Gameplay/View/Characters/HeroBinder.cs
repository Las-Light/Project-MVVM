using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    public class HeroBinder : MonoBehaviour
    {
        public CharacterController CharacterController;
        private Camera _mainCamera;
        private HeroViewModel _viewModel;

        public void Bind(HeroViewModel viewModel)
        {
            _viewModel = viewModel; 
            var currentPosOnMap = viewModel.CurrentMap;
            transform.position = currentPosOnMap.CurrentValue.Position.Value;
            CharacterController = GetComponent<CharacterController>();
            viewModel.SetHeroView(this);
        }

        private void Update()
        {
            _viewModel.Move();
        }

        public bool IsInteractiveActionPressed()
        {
            return _viewModel.InteractiveActionPressed();
        }
    }
}