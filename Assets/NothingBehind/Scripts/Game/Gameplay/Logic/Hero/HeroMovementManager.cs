using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Hero
{
    public class HeroMovementManager
    {
        private readonly HeroSettings _heroSettings;
        private readonly GameplayInputManager _inputManager;

        private CharacterController _heroCharacterController;
        private Transform _mainCameraTransform;
        private Transform _heroTransform;
        private AnimatorManager _animatorManager;

        private float _speed;
        private float _targetRotation;
        private float _speedBlend;
        private float _speedBlendX;
        private float _speedBlendY;
        private float _rotationVelocity;
        private float _fallTimeoutDelta;
        private float _verticalVelocity;
        private bool _grounded;
        private bool _isCrouch;

        public HeroMovementManager(HeroSettings heroSettings,
            GameplayInputManager inputManager)
        {
            _inputManager = inputManager;
            _heroSettings = heroSettings;

            _inputManager.IsCrouch.Skip(1).Subscribe(_ => Crouch());
        }

        public void BindHeroViewComponent(HeroBinder heroView, Camera mainCamera, CharacterController controller)
        {
            _heroCharacterController = controller;
            _mainCameraTransform = mainCamera.transform;
            _heroTransform = heroView.transform;
            _animatorManager = heroView.GetComponent<AnimatorManager>();
        }

        //TODO: убрать в другой сервис
        public bool InteractiveActionPressed()
        {
            return _inputManager.IsInteract.CurrentValue;
        }

        //метод управляет движением игрока

        public void Move()
        {
            GroundedCheck();
            FreeFall();
            var moveDirection = _inputManager.Move.CurrentValue;
            SwitchSpeed(moveDirection);
            if (moveDirection != Vector2.zero)
            {
                MovementAction(moveDirection);
                if (_inputManager.IsAim.CurrentValue)
                {
                    MoveAimDirection(moveDirection);
                }
                else
                {
                    MoveForwardDirection();
                }
            }
            else
            {
                if (_inputManager.IsAim.CurrentValue)
                {
                    _animatorManager.AimMove(0, 0);
                }
                else
                {
                    _animatorManager.Move(_speedBlend);
                }
            }
        }

        //метод который задает анимацию приседания при нажатии на кнопку "присесть", а также меняет поле isCrouch
        //используется поле isCrouch для определения находится ли сейчас в режиме "присед", в нем же меняет высоту капсулы
        //коллайдера CharacterController и меняет высоту точки проверки на препятствие при режиме "присед" и "стоя"
        private void Crouch()
        {
            _isCrouch = !_isCrouch;

            if (_isCrouch)
            {
                //pointToCheckClip.localPosition = new Vector3(0.2f, 0.95f, 0);
                //TargetPointForAim.localPosition = new Vector3(0, 0.9f, 0);
                _animatorManager.Crouch(_isCrouch);
                _heroCharacterController.height = _heroSettings.CrouchHeight;
                _heroCharacterController.center = _heroSettings.crouchCenter;
            }
            else
            {
                //pointToCheckClip.localPosition = new Vector3(0.2f, 1.6f, 0);
                //TargetPointForAim.localPosition = new Vector3(0, 1.4f, 0);
                _animatorManager.Crouch(_isCrouch);
                _heroCharacterController.height = _heroSettings.DefaultHeight;
                _heroCharacterController.center = _heroSettings.DefaultCenter;
            }
        }

        //метод позволяет направлять персонажа относительно того куда он смотрит, а также передает в аниматор
        //сглаженые движения и задает анимацию в соответствии с направлениям движения

        private void MoveAimDirection(Vector2 movementDirection)
        {
            Vector3 cameraF = _mainCameraTransform.forward;
            Vector3 cameraR = _mainCameraTransform.right;

            cameraF.y = 0;
            cameraR.y = 0;

            Vector3 moveDirectional =
                cameraF.normalized * movementDirection.y + cameraR.normalized * movementDirection.x;
            moveDirectional = Vector3.ClampMagnitude(moveDirectional, 1);

            Vector3 relativeVector = _heroTransform.InverseTransformDirection(moveDirectional);

            _speedBlendX = Mathf.Lerp(_speedBlendX, relativeVector.x, Time.deltaTime * _heroSettings.SpeedBlendAim);
            _speedBlendY = Mathf.Lerp(_speedBlendY, relativeVector.z, Time.deltaTime * _heroSettings.SpeedBlendAim);

            _animatorManager.AimMove(_speedBlendX, _speedBlendY);
        }

        //метод задает направление движение игрока без прицеливания

        private void MoveForwardDirection()
        {
            float rotation = Mathf.SmoothDampAngle(_heroTransform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _heroSettings.RotationSmoothTime);

            // повернуться лицом в соответствии с заданым направлением левым стиком относительно камеры,
            // если в данный момент не нажата кнопка "прицелиться"
            _heroTransform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            // update animator if using character
            _animatorManager.Move(_speedBlend);
        }

        private void MovementAction(Vector2 moveDirection)
        {
            SwitchSpeed(moveDirection);

            if (moveDirection != Vector2.zero)
            {
                Vector3 inputDirection = new Vector3(moveDirection.x, 0.0f, moveDirection.y);

                // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                // if there is a move input rotate player when the player is moving
                _targetRotation =
                    Mathf.Atan2(inputDirection.x, inputDirection.z)
                    * Mathf.Rad2Deg
                    + _mainCameraTransform.eulerAngles.y;

                Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

                // move direction the player
                _heroCharacterController.Move(targetDirection * (_speed * Time.deltaTime) +
                                              new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
        }

        private void SwitchSpeed(Vector2 movementDir)
        {
            // устанавливает targetSpeed в зависимости от нажатия кнопки "прицелиться", "бежать" или "присесть"
            float targetSpeed;

            if (_inputManager.IsAim.CurrentValue)
            {
                targetSpeed = _heroSettings.AimSpeed;
            }
            else if (_inputManager.IsSprint.CurrentValue)
            {
                targetSpeed = _heroSettings.SprintSpeed;
                _isCrouch = false;
                //TODO: т.к. Crouch не в Update игрок не встает если нажать кропку спринт
                _animatorManager.Crouch(_isCrouch);
            }
            else if (_isCrouch)
            {
                targetSpeed = _heroSettings.CrouchSpeed;
            }
            else
            {
                targetSpeed = _heroSettings.MoveSpeed;
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (movementDir == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            var velocity = _heroCharacterController.velocity;
            float currentHorizontalSpeed =
                new Vector3(velocity.x, 0.0f, velocity.z).magnitude;

            float speedOffset = 0.1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                    Time.deltaTime * _heroSettings.SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _speedBlend = Mathf.Lerp(_speedBlend, targetSpeed, Time.deltaTime * _heroSettings.SpeedChangeRate);
            if (_speedBlend < 0.01f) _speedBlend = 0f;
        }

        //метод применяет к игроку гравитацию если находится в свободном падении
        private void FreeFall()
        {
            if (_grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = _heroSettings.FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // update animator if using character
                _animatorManager.FreeFall(false);
            }
            else
            {
                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    _animatorManager.FreeFall(true);
                }
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _heroSettings.TerminalVelocity)
            {
                _verticalVelocity += _heroSettings.Gravity * Time.deltaTime;
            }
        }

        //метод проверяет находится игрок на земле и передает эти данные в аниматор
        private void GroundedCheck()
        {
            // set sphere position, with offset
            var position = _heroTransform.position;
            Vector3 spherePosition = new Vector3(position.x,
                position.y - _heroSettings.GroundedOffset,
                position.z);
            _grounded = Physics.CheckSphere(spherePosition,
                _heroSettings.GroundedRadius,
                _heroSettings.GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            _animatorManager.Grounded(_grounded);
        }
    }
}