using LitMotion;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Player;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Camera;
using NothingBehind.Scripts.Game.Settings.Gameplay;
using R3;
using Unity.Cinemachine;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Camera
{
    public class BGCameraView : MonoBehaviour
    {
        [SerializeField] private float transitionDuration = 0.5f;

        private CameraViewModel _cameraViewModel;
        private GameplayCameraSettings _cameraSettings;
        private CinemachineCamera _cinemachineCamera;
        private CinemachineCameraOffset _cinemachineCameraOffset;
        private CinemachinePanTilt _cinemachineRotation;
        private CinemachinePositionComposer _cinemachinePosition;
        private PlayerView _playerView;

        private ReadOnlyReactiveProperty<bool> _isRotateCameraLeft;
        private ReadOnlyReactiveProperty<bool> _isRotateCameraRight;
        private ReadOnlyReactiveProperty<bool> _isAim;
        private Vector3 _aimDirection;
        private Vector3 _mousePosition;

        private CompositeDisposable _disposables = new();

        private MotionHandle _rotationMotionHandle;
        private MotionHandle _aimMotionHandle;

        public void Bind(CameraViewModel cameraViewModel, PlayerView playerView)
        {
            _cameraViewModel = cameraViewModel;
            _cameraSettings = cameraViewModel.CameraSettings;
            _cinemachineCamera = GetComponent<CinemachineCamera>();
            _cinemachineCameraOffset = GetComponent<CinemachineCameraOffset>();
            _cinemachineRotation = GetComponent<CinemachinePanTilt>();
            _cinemachinePosition = GetComponent<CinemachinePositionComposer>();
            _cinemachineCamera.Follow = playerView.transform;
            _playerView = playerView;
            _isRotateCameraLeft = cameraViewModel.InputManager.IsRotateCameraLeft;
            _isRotateCameraRight = cameraViewModel.InputManager.IsRotateCameraRight;
            _isAim = cameraViewModel.InputManager.IsAim;
            
            playerView.GetComponent<LookPlayerController>().MouseWorldPosition
                .Subscribe(value => { _mousePosition = value; })
                .AddTo(_disposables);
            _isRotateCameraLeft.Skip(1).Subscribe(_ => { RotateBy45DegWithLitMotion(-45f); }).AddTo(_disposables);
            _isRotateCameraRight.Skip(1).Subscribe(_ => { RotateBy45DegWithLitMotion(45f); }).AddTo(_disposables);
            _isAim.Skip(1).Subscribe(isAim => { if (!isAim) ResetCamera(); }).AddTo(_disposables);
        }

        private void Update()
        {
            if (!_isAim.CurrentValue)
            {
                // Если игрок не прицеливается, сбрасываем камеру и выходим
                return;
            }

            // Кэшируем направление мыши относительно игрока
            var aimDirection = _mousePosition - _playerView.transform.position;
            float distanceSquared = aimDirection.sqrMagnitude;

            if (distanceSquared >= 80f)
            {
                // Смещаем камеру в направлении прицеливания
                FollowCameraToAimDirection(_playerView.ArsenalView.CurrentWeapon.AimRange, _playerView.transform);
            }
            else
            {
                // Возвращаем камеру в исходное положение
                ResetCamera();
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// Смещает камеру в направлении прицеливания.
        /// </summary>
        /// <param name="aimingRange">Дистанция прицеливания оружия.</param>
        /// <param name="playerTransform">Трансформ игрока.</param>
        private void FollowCameraToAimDirection(float aimingRange, Transform playerTransform)
        {
            if (_cinemachineCameraOffset == null || _cameraSettings == null) return;

            // 1. Получаем направление прицела в мировых координатах (от игрока к курсору)
            _aimDirection = playerTransform.forward;

            // 2. Преобразуем мировое направление в локальное пространство камеры
            Vector3 localAimDirection = transform.InverseTransformDirection(_aimDirection);

            // 3. Умножаем на силу смещения
            localAimDirection *= aimingRange;

            // 4. Преобразуем в новый вектор для CameraOffset (там смещение по х и у)
            var offset = new Vector3(localAimDirection.x, localAimDirection.z, 0f);

            SetOffset(offset, 1);
        }

        /// <summary>
        /// Возвращает камеру в исходное положение.
        /// </summary>
        private void ResetCamera()
        {
            if (_cinemachineCameraOffset == null) return;
            if (_cinemachinePosition.Composition.ScreenPosition != Vector2.zero) 
                _cinemachinePosition.Composition.ScreenPosition = Vector2.zero;

            ResetOffset();
        }

        private void ResetOffset(float duration = 1f)
        {
            SetOffset(Vector3.zero, duration);
        }

        private void SetOffset(Vector3 targetOffset, float duration = -1f)
        {
            if (duration < 0) duration = transitionDuration;
            _aimMotionHandle.TryCancel();

            _aimMotionHandle = LMotion.Create(_cinemachineCameraOffset.Offset, targetOffset, duration)
                .WithEase(Ease.OutCubic)
                .Bind(value => { _cinemachineCameraOffset.Offset = value; })
                .AddTo(gameObject);
        }

        private void RotateBy45DegWithLitMotion(float axisRotation)
        {
            if (_cinemachineCameraOffset == null || _rotationMotionHandle.IsActive())
                return;

            var targetRotation = _cinemachineRotation.PanAxis.Value + axisRotation;
            _rotationMotionHandle = LMotion.Create(_cinemachineRotation.PanAxis.Value, targetRotation, 0.5f)
                .WithEase(Ease.OutCubic)
                .Bind(value => _cinemachineRotation.PanAxis.Value = value)
                .AddTo(gameObject);
        }
    }
}