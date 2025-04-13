using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Player
{
    public class MovementController : MonoBehaviour
    {
        [Header("Aim")] 
        [Tooltip("TargetAimPoint")]
        [SerializeField] private Transform targetPointForAim;
        [Header("Clip Prevention")] 
        [Tooltip("ViewPoint")][SerializeField]
        private Transform pointToCheckClip;
        
        private PlayerSettings _playerSettings;
        private GameplayInputManager _inputManager;

        private PlayerView _playerView;
        private CharacterController _playerCharacterController;
        private Transform _mainCameraTransform;
        private AnimatorController _animatorController;
        private readonly CompositeDisposable _disposables = new();

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

        private void Start()
        {
            _playerView = GetComponent<PlayerView>();
            _animatorController = GetComponent<AnimatorController>();
            if (Camera.main != null) _mainCameraTransform = Camera.main.transform;
            _playerCharacterController = GetComponent<CharacterController>();
            _playerSettings = _playerView.PlayerSettings;
            _inputManager = _playerView.InputManager;
            _disposables.Add(_inputManager.IsCrouch.Skip(1).Subscribe(_ => Crouch()));
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
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
                    _animatorController.AimMove(0, 0);
                }
                else
                {
                    _animatorController.Move(_speedBlend);
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
                pointToCheckClip.localPosition = new Vector3(0.2f, 0.95f, 0);
                targetPointForAim.localPosition = new Vector3(0, 0.9f, 0);
                _animatorController.Crouch(_isCrouch);
                _playerCharacterController.height = _playerSettings.CrouchHeight;
                _playerCharacterController.center = _playerSettings.crouchCenter;
            }
            else
            {
                pointToCheckClip.localPosition = new Vector3(0.2f, 1.6f, 0);
                targetPointForAim.localPosition = new Vector3(0, 1.4f, 0);
                _animatorController.Crouch(_isCrouch);
                _playerCharacterController.height = _playerSettings.DefaultHeight;
                _playerCharacterController.center = _playerSettings.DefaultCenter;
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

            Vector3 relativeVector = transform.InverseTransformDirection(moveDirectional);

            _speedBlendX = Mathf.Lerp(_speedBlendX, relativeVector.x, Time.deltaTime * _playerSettings.SpeedBlendAim);
            _speedBlendY = Mathf.Lerp(_speedBlendY, relativeVector.z, Time.deltaTime * _playerSettings.SpeedBlendAim);

            _animatorController.AimMove(_speedBlendX, _speedBlendY);
        }

        //метод задает направление движение игрока без прицеливания

        private void MoveForwardDirection()
        {
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _playerSettings.RotationSmoothTime);

            // повернуться лицом в соответствии с заданым направлением левым стиком относительно камеры,
            // если в данный момент не нажата кнопка "прицелиться"
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            // update animator if using character
            _animatorController.Move(_speedBlend);
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
                _playerCharacterController.Move(targetDirection * (_speed * Time.deltaTime) +
                                              new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
        }

        private void SwitchSpeed(Vector2 movementDir)
        {
            // устанавливает targetSpeed в зависимости от нажатия кнопки "прицелиться", "бежать" или "присесть"
            float targetSpeed;

            if (_inputManager.IsAim.CurrentValue)
            {
                targetSpeed = _playerSettings.AimSpeed;
            }
            else if (_inputManager.IsSprint.CurrentValue)
            {
                targetSpeed = _playerSettings.SprintSpeed;
                _isCrouch = false;
                //TODO: т.к. Crouch не в Update игрок не встает если нажать кропку спринт
                _animatorController.Crouch(_isCrouch);
            }
            else if (_isCrouch)
            {
                targetSpeed = _playerSettings.CrouchSpeed;
            }
            else
            {
                targetSpeed = _playerSettings.MoveSpeed;
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (movementDir == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            var velocity = _playerCharacterController.velocity;
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
                    Time.deltaTime * _playerSettings.SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _speedBlend = Mathf.Lerp(_speedBlend, targetSpeed, Time.deltaTime * _playerSettings.SpeedChangeRate);
            if (_speedBlend < 0.01f) _speedBlend = 0f;
        }

        //метод применяет к игроку гравитацию если находится в свободном падении
        private void FreeFall()
        {
            if (_grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = _playerSettings.FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // update animator if using character
                _animatorController.FreeFall(false);
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
                    _animatorController.FreeFall(true);
                }
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _playerSettings.TerminalVelocity)
            {
                _verticalVelocity += _playerSettings.Gravity * Time.deltaTime;
            }
        }

        //метод проверяет находится игрок на земле и передает эти данные в аниматор
        private void GroundedCheck()
        {
            // set sphere position, with offset
            var position = transform.position;
            Vector3 spherePosition = new Vector3(position.x,
                position.y - _playerSettings.GroundedOffset,
                position.z);
            _grounded = Physics.CheckSphere(spherePosition,
                _playerSettings.GroundedRadius,
                _playerSettings.GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            _animatorController.Grounded(_grounded);
        }
    }
}