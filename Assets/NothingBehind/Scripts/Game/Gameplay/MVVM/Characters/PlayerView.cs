using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Characters
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerView : MonoBehaviour
    {
        private PlayerViewModel _viewModel;

        public void Bind(PlayerViewModel viewModel, GameplayUIManager gameplayUIManager)
        {
            _viewModel = viewModel;
            var currentMap = viewModel.CurrentMapId.CurrentValue;
            var currentPosOnMap = viewModel.PositionOnMaps.First(posOnMap => posOnMap.MapId == currentMap);
            transform.position = currentPosOnMap.Position.Value;
            var mainCamera = Camera.main;
            viewModel.SetPlayerViewWithComponent(this, mainCamera);
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