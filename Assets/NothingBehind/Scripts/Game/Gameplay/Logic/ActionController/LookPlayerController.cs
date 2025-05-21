using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using Math = System.Math;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Player
{
    public class LookPlayerController : MonoBehaviour
    {
        public bool AimAssistON;

        private Camera _mainCamera;
        private PlayerInput _playerInput;
        private AnimatorController _animatorController;
        private AimController _aimController;

        public ReactiveProperty<Vector3> MouseWorldPosition = new();
        private float _turnRotation;
        private GameplayInputManager _inputManager;
        private PlayerSettings _playerSettings;
        private PlayerView _playerView;

        private void Start()
        {
            _mainCamera = Camera.main;
            _playerInput = GetComponent<PlayerInput>();
            _animatorController = GetComponent<AnimatorController>();
            _playerView = GetComponent<PlayerView>();
            _inputManager = _playerView.InputManager;
            _playerSettings = _playerView.PlayerSettings;
            _aimController = GetComponent<AimController>();
        }

        public void Look()
        {
            if (_playerInput.currentControlScheme == AppConstants.GamepadControlScheme)
                LookGamepad();

            if (_playerInput.currentControlScheme == AppConstants.KeyboardMouseControlScheme)
                LookMouse();
        }

        //метод задает направление игрока когда игрок не движется
        private void LookMouse()
        {
            if (_playerInput.currentControlScheme != AppConstants.KeyboardMouseControlScheme)
                return;

            var moveDirection = _inputManager.Move.CurrentValue;
            if ((_inputManager.MouseIsActive && moveDirection == Vector2.zero) ||
                _inputManager.IsAim.CurrentValue)
            {
                MouseWorldPosition.Value = Vector3.zero;

                Ray ray = _mainCamera.ScreenPointToRay(_inputManager.LookMouse.CurrentValue);
                if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, _playerSettings.AimColliderLayerMask))
                {
                    MouseWorldPosition.Value =
                        CalculateAimMousePosition(raycastHit.point, _inputManager.LookMouse.CurrentValue);

                    Vector3 worldAimTarget = MouseWorldPosition.Value;
                    var position = transform.position;
                    worldAimTarget.y = position.y;
                    Vector3 aimDirection = (worldAimTarget - position).normalized;

                    //определяет влево или вправо совершил поворот игрок -90 или 90
                    _turnRotation = Vector3.SignedAngle(aimDirection, transform.forward, transform.up);

                    //если мышь не активна, то персонаж не поворачивается в её сторону
                    transform.rotation = Quaternion.RotateTowards(transform.rotation,
                        Quaternion.LookRotation(aimDirection),
                        _playerSettings.MouseRotationSpeed * Time.deltaTime);
                    AimPointTargetMouse(MouseWorldPosition.Value);
                }

                //анимация поворота на месте
                if (moveDirection == Vector2.zero)
                {
                    _animatorController.Turn(_turnRotation, (int)_turnRotation);
                }
                else
                {
                    _animatorController.Turn(0, 0);
                }
            }
            else
            {
                _animatorController.Turn(0f, 0);
            }
        }

        //метод позволяет поворачивать игрока лицом в направлении движения правого стика геймпада
        //матрица применяется для уточнения направления с учетом поворота камеры 
        private void LookGamepad()
        {
            if (_playerInput.currentControlScheme != AppConstants.GamepadControlScheme)
                return;
            if ((_inputManager.LookGamepad.CurrentValue != Vector2.zero &&
                 _inputManager.Move.CurrentValue == Vector2.zero) ||
                (_inputManager.IsAim.CurrentValue && _inputManager.LookGamepad.CurrentValue != Vector2.zero))
            {
                Matrix4x4 matrix = Matrix4x4.Rotate(Quaternion.Euler(0, _mainCamera.transform.eulerAngles.y, 0));

                var aimDirection = new Vector3(_inputManager.LookGamepad.CurrentValue.x, 0.0f,
                    _inputManager.LookGamepad.CurrentValue.y);


                Vector3 skewedInput = matrix.MultiplyPoint3x4(aimDirection);


                // поворот на месте на 180 градусов (без анимации)
                _turnRotation = Vector3.SignedAngle(skewedInput.normalized, transform.forward, transform.up);
                float turnRotAbs = Mathf.Abs(_turnRotation);

                var destinationRotation = Quaternion.LookRotation(skewedInput);
                if (turnRotAbs > 170)
                    transform.rotation = destinationRotation;

                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    destinationRotation,
                    _playerSettings.GamepadRotationSpeed * Time.deltaTime);
                //StartCoroutine(RotationToSkewedInput(skewedInput));

                AimPointTargetGamepad();

                // выбор скорости вращения игрока стиком с учетом на какрй угол происходит поворот
                // чем меньше угол тем плавнее поворачивается игрок
                if (turnRotAbs < 60)
                    _playerSettings.GamepadRotationSpeed = 50;
                else
                    _playerSettings.GamepadRotationSpeed = 90;

                // анимация поворота на месте 
                if (_inputManager.Move.CurrentValue == Vector2.zero && turnRotAbs >= 45)
                    _animatorController.Turn(_turnRotation, (int)_turnRotation);
                else
                    _animatorController.Turn(0, 0);
            }
            else
                _animatorController.Turn(0, 0);
        }

        //направляет AimPoint к близшайшей цели, если целей нет в зоне видимости то AimPoint возвращается дефолтное состояние
        private void AimPointTargetGamepad()
        {
            if (AimAssistON)
            {
                if (_playerView.CurrentEnemy)
                    _aimController.SetAimPointPosition(_playerView.CurrentEnemy.transform.GetChild(0).position,
                        _playerView.ArsenalView.ActiveGun);
                else
                    _aimController.SetAimPointForward();
            }
            else
                _aimController.SetAimPointForward();
        }

        private void AimPointTargetMouse(Vector3 mouseWorldPosition)
        {
            if (AimAssistON)
            {
                if (_playerView.CurrentEnemy)
                    _aimController.SetAimPointPosition(_playerView.CurrentEnemy.transform.GetChild(0).position,
                        _playerView.ArsenalView.ActiveGun);
                else
                    _aimController.SetAimPointPosition(mouseWorldPosition, _playerView.ArsenalView.ActiveGun);
            }
            else
                _aimController.SetAimPointPosition(mouseWorldPosition, _playerView.ArsenalView.ActiveGun);
        }

        private Vector3 CalculateAimMousePosition(Vector3 hit, Vector2 mousePos)
        {
            Vector3 mouseScreenToWorld = _mainCamera.ScreenToWorldPoint(mousePos);
            var yTotal = Math.Round(hit.y - mouseScreenToWorld.y, 6);
            var newY = Math.Round(yTotal - (hit.y - (hit.y + _playerSettings.WeaponHeightOffset.y)), 6);
            var factor = (float)Math.Round(newY / yTotal, 6);
            Vector3 targetPos = mouseScreenToWorld + ((hit - mouseScreenToWorld) * factor);
            return targetPos;
        }
    }
}