using System;
using System.Collections;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Settings.Gameplay;
using R3;
using Unity.Cinemachine;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM
{
    public class CameraView : MonoBehaviour
    {
        private CameraViewModel _cameraViewModel;
        private GameplayCameraSettings _cameraSettings;
        private CinemachineCamera _cinemachineCamera;
        private CinemachineFollow _cinemachineFollow;
        private Camera _cameraMain;
        private PlayerView _playerView;

        private ReadOnlyReactiveProperty<bool> _isRotateCameraLeft;
        private ReadOnlyReactiveProperty<bool> _isRotateCameraRight;
        private ReadOnlyReactiveProperty<bool> _isAim;
        private bool _progressRotate;
        private bool _isFollowing;

        private CompositeDisposable _disposables = new();
        private Vector3 _mousePosition;

        public void Bind(CameraViewModel cameraViewModel, PlayerView playerView)
        {
            _cameraViewModel = cameraViewModel;
            _cameraSettings = cameraViewModel.CameraSettings;
            _cameraMain = Camera.main;
            _cinemachineCamera = GetComponent<CinemachineCamera>();
            _cinemachineFollow = GetComponent<CinemachineFollow>();
            _cinemachineCamera.Follow = playerView.transform;
            _playerView = playerView;
            _isRotateCameraLeft = cameraViewModel.InputManager.IsRotateCameraLeft;
            _isRotateCameraRight = cameraViewModel.InputManager.IsRotateCameraRight;
            _isAim = cameraViewModel.InputManager.IsAim;
            playerView.GetComponent<LookPlayerController>().MouseWorldPosition.Subscribe(value => { _mousePosition = value; })
                .AddTo(_disposables);
            _disposables.Add(_isRotateCameraLeft.Skip(1).Subscribe(_ => StartCoroutine(RotateBy45Deg(Vector3.up))));
            _disposables.Add(_isRotateCameraRight.Skip(1).Subscribe(_ => StartCoroutine(RotateBy45Deg(Vector3.down))));
        }

        private void Update()
        {
            if (!_isAim.CurrentValue)
            {
                // Если игрок не прицеливается, сбрасываем камеру и выходим
                ResetCamera();
                return;
            }

            // Кэшируем направление мыши относительно игрока
            Vector3 aimDirection = _mousePosition - _playerView.transform.position;
            float distanceSquared = aimDirection.sqrMagnitude;

            if (distanceSquared >= 80f)
            {
                // Смещаем камеру в направлении прицеливания
                FollowCameraToAimDirection(_playerView.ArsenalView.ActiveGun.AimRange, _playerView.transform);
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
        public void FollowCameraToAimDirection(float aimingRange, Transform playerTransform)
        {
            if (_cinemachineFollow == null || _cameraSettings == null) return;

            // Рассчитываем новое смещение камеры
            Vector3 targetOffset = playerTransform.forward * aimingRange;

            // Проверяем, нужно ли обновлять смещение
            if (Vector3.SqrMagnitude(_cinemachineFollow.FollowOffset - targetOffset) < 0.01f)
            {
                // Если текущее смещение близко к целевому, прекращаем интерполяцию
                _cinemachineFollow.FollowOffset = targetOffset;
                return;
            }

            // Плавно изменяем смещение камеры
            _cinemachineFollow.FollowOffset = Vector3.Lerp(
                _cinemachineFollow.FollowOffset,
                targetOffset,
                Time.deltaTime * _cameraSettings.FollowSpeed
            );
        }

        /// <summary>
        /// Возвращает камеру в исходное положение.
        /// </summary>
        public void ResetCamera()
        {
            if (_cinemachineFollow == null || _cameraSettings == null || _cinemachineFollow.FollowOffset == Vector3.zero) return;

            // Проверяем, нужно ли обновлять смещение
            if (Vector3.SqrMagnitude(_cinemachineFollow.FollowOffset) < 0.01f)
            {
                // Если смещение уже близко к нулю, прекращаем интерполяцию
                _cinemachineFollow.FollowOffset = Vector3.zero;
                return;
            }

            // Плавно возвращаем смещение камеры к нулевому значению
            _cinemachineFollow.FollowOffset = Vector3.Lerp(
                _cinemachineFollow.FollowOffset,
                Vector3.zero,
                Time.deltaTime * _cameraSettings.FollowSpeed
            );
        }

        private IEnumerator RotateBy45Deg(Vector3 rotDir)
        {
            if (_progressRotate)
            {
                yield break;
            }

            _progressRotate = true;
            float t = 0;
            Quaternion start = transform.rotation;
            Quaternion target = Quaternion.AngleAxis(45, rotDir) * transform.localRotation;

            do
            {
                if (transform == null)
                    yield break;

                t += _cameraSettings.RotationSpeed / 45f * Time.deltaTime;

                transform.rotation = Quaternion.Lerp(start, target, t);
                yield return null;
            } while (t < 1f);

            _progressRotate = false;
        }
    }
}