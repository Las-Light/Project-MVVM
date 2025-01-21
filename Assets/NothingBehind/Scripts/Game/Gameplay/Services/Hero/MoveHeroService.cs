using NothingBehind.Scripts.Game.Gameplay.Services.InputManager;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services.Hero
{
    public class MoveHeroService
    {
        private readonly HeroSettings _heroSettings;
        private readonly GameplayInputManager _inputManager;
        
        private float _speed = 4.0f;

        public MoveHeroService(HeroSettings heroSettings, GameplayInputManager inputManager)
        {
            _inputManager = inputManager;
            _heroSettings = heroSettings;
        }


        public Vector3 Move()
        {
            var moveDirection = _inputManager.Move.CurrentValue;

            Vector3 inputDirection = new Vector3(moveDirection.x, 0.0f, moveDirection.y);

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            var targetRotation =
                Mathf.Atan2(inputDirection.x, inputDirection.z); //* Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;

            Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

            // move direction the player
            return inputDirection * (_speed * Time.deltaTime);
        }

        public bool InteractiveActionPressed()
        {
            return _inputManager.IsInteract.CurrentValue;
        }
    }
}