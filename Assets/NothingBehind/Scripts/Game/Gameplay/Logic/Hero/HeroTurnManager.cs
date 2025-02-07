using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using UnityEngine;
using UnityEngine.InputSystem;
using Math = System.Math;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Hero
{
    public class HeroTurnManager
    {
        private readonly GameplayInputManager _inputManager;
        private readonly HeroSettings _heroSettings;

        private HeroBinder _heroView;
        private Camera _mainCamera;
        private PlayerInput _playerInput;
        private AnimatorManager _animatorManager;

        private Vector3 _mouseWorldPosition;
        private float _turnRotation;

        public HeroTurnManager(GameplayInputManager inputManager,
            HeroSettings heroSettings)
        {
            _inputManager = inputManager;
            _heroSettings = heroSettings;
        }

        public void BindHeroViewComponent(HeroBinder heroView, Camera mainCamera, PlayerInput playerInput)
        {
            _heroView = heroView;
            _mainCamera = mainCamera;
            _playerInput = playerInput;
            _animatorManager = heroView.GetComponent<AnimatorManager>();
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
                if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, _heroSettings.AimColliderLayerMask))
                {
                    _mouseWorldPosition =
                        CalculateAimMousePosition(raycastHit.point, _inputManager.LookMouse.CurrentValue);

                    Vector3 worldAimTarget = _mouseWorldPosition;
                    var transform = _heroView.transform;
                    worldAimTarget.y = transform.position.y;
                    Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

                    //определяет влево или вправо совершил поворот игрок -90 или 90
                    _turnRotation = Vector3.SignedAngle(aimDirection, transform.forward, transform.up);

                    //если мышь не активна, то персонаж не поворачивается в её сторону
                    transform.rotation = Quaternion.RotateTowards(transform.rotation,
                        Quaternion.LookRotation(aimDirection),
                        _heroSettings.MouseRotationSpeed * Time.deltaTime);
                    //_aimController.AimPointTargetMouse(raycastHit, _mouseWorldPosition, _weaponController.ActiveGun);
                }

                //анимация поворота на месте
                if (moveDirection == Vector2.zero)
                {
                    _animatorManager.Turn(_turnRotation, (int)_turnRotation);
                }
                else
                {
                    _animatorManager.Turn(0, 0);
                }
            }
            else
            {
                _animatorManager.Turn(0f, 0);
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
                var transform = _heroView.transform;
                _turnRotation = Vector3.SignedAngle(skewedInput.normalized, transform.forward, transform.up);
                float turnRotAbs = Mathf.Abs(_turnRotation);

                var destinationRotation = Quaternion.LookRotation(skewedInput);
                if (turnRotAbs > 170)
                    transform.rotation = destinationRotation;

                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    destinationRotation,
                    _heroSettings.GamepadRotationSpeed * Time.deltaTime);
                //StartCoroutine(RotationToSkewedInput(skewedInput));

                //_aimController.AimPointTargetGamepad(_weaponController.ActiveGun);

                // выбор скорости вращения игрока стиком с учетом на какрй угол происходит поворот
                // чем меньше угол тем плавнее поворачивается игрок
                if (turnRotAbs < 60)
                    _heroSettings.GamepadRotationSpeed = 50;
                else
                    _heroSettings.GamepadRotationSpeed = 90;

                // анимация поворота на месте 
                if (_inputManager.Move.CurrentValue == Vector2.zero && turnRotAbs >= 45)
                    _animatorManager.Turn(_turnRotation, (int)_turnRotation);
                else
                    _animatorManager.Turn(0, 0);
            }
            else
                _animatorManager.Turn(0, 0);
        }

        private Vector3 CalculateAimMousePosition(Vector3 hit, Vector2 mousePos)
        {
            Vector3 mouseScreenToWorld = _mainCamera.ScreenToWorldPoint(mousePos);
            var yTotal = Math.Round(hit.y - mouseScreenToWorld.y, 6);
            var newY = Math.Round(yTotal - (hit.y - (hit.y + _heroSettings.WeaponHeightOffset.y)), 6);
            var factor = (float)Math.Round(newY / yTotal, 6);
            Vector3 targetPos = mouseScreenToWorld + ((hit - mouseScreenToWorld) * factor);
            return targetPos;
        }
    }
}