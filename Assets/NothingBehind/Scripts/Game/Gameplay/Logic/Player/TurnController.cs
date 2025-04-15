using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using UnityEngine;
using UnityEngine.InputSystem;
using Math = System.Math;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Player
{
    public class TurnController: MonoBehaviour
    {
        private Camera _mainCamera;
        private PlayerInput _playerInput;
        private AnimatorController _animatorController;
        private AimController _aimController;

        private Vector3 _mouseWorldPosition;
        private float _turnRotation;
        private GameplayInputManager _inputManager;
        private PlayerSettings _playerSettings;

        private void Start()
        {
            _mainCamera = Camera.main;
            _playerInput = GetComponent<PlayerInput>();
            _animatorController = GetComponent<AnimatorController>();
            _inputManager = GetComponent<PlayerView>().InputManager;
            _playerSettings = GetComponent<PlayerView>().PlayerSettings;
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
        public void LookMouse()
        {
            if (_playerInput.currentControlScheme != AppConstants.KeyboardMouseControlScheme)
                return;

            var moveDirection = _inputManager.Move.CurrentValue;
            if ((_inputManager.MouseIsActive && moveDirection == Vector2.zero) ||
                _inputManager.IsAim.CurrentValue)
            {
                _mouseWorldPosition = Vector3.zero;

                Ray ray = _mainCamera.ScreenPointToRay(_inputManager.LookMouse.CurrentValue);
                if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, _playerSettings.AimColliderLayerMask))
                {
                    _mouseWorldPosition =
                        CalculateAimMousePosition(raycastHit.point, _inputManager.LookMouse.CurrentValue);

                    Vector3 worldAimTarget = _mouseWorldPosition;
                    var position = transform.position;
                    worldAimTarget.y = position.y;
                    Vector3 aimDirection = (worldAimTarget - position).normalized;

                    //определяет влево или вправо совершил поворот игрок -90 или 90
                    _turnRotation = Vector3.SignedAngle(aimDirection, transform.forward, transform.up);

                    //если мышь не активна, то персонаж не поворачивается в её сторону
                    transform.rotation = Quaternion.RotateTowards(transform.rotation,
                        Quaternion.LookRotation(aimDirection),
                        _playerSettings.MouseRotationSpeed * Time.deltaTime);
                    _aimController.AimPointTargetMouse(raycastHit, _mouseWorldPosition);
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
        public void LookGamepad()
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

                _aimController.AimPointTargetGamepad();

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