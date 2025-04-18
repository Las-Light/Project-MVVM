using System;
using System.Collections;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Settings.Gameplay;
using R3;
using Unity.Cinemachine;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM
{
    public class CameraView: MonoBehaviour
    {
        private CameraViewModel _cameraViewModel;
        private GameplayCameraSettings _cameraSettings;
        private CinemachineCamera _cinemachineCamera;
        private CinemachineFollow _cinemachineFollow;
        
        private ReadOnlyReactiveProperty<bool> _isRotateCameraLeft;
        private ReadOnlyReactiveProperty<bool> _isRotateCameraRight;
        private ReadOnlyReactiveProperty<bool> _isAim;
        private bool _progressRotate;

        private CompositeDisposable _disposables = new();

        public void Bind(CameraViewModel cameraViewModel, PlayerView playerView)
        {
            _cameraViewModel = cameraViewModel;
            _cameraSettings = cameraViewModel.CameraSettings;
            _cinemachineCamera = GetComponent<CinemachineCamera>();
            _cinemachineFollow = GetComponent<CinemachineFollow>();
            _cinemachineCamera.Follow = playerView.transform;
            _isRotateCameraLeft = cameraViewModel.InputManager.IsRotateCameraLeft;
            _isRotateCameraRight = cameraViewModel.InputManager.IsRotateCameraRight;
            _isAim = cameraViewModel.InputManager.IsAim;
            _disposables.Add(_isRotateCameraLeft.Skip(1).Subscribe(_ => StartCoroutine(RotateBy45Deg(Vector3.up))));
            _disposables.Add(_isRotateCameraRight.Skip(1).Subscribe(_ => StartCoroutine(RotateBy45Deg(Vector3.down))));
            _disposables.Add(_isAim.Skip(1).Subscribe(isAim =>
            {
                if (isAim)
                {
                    FollowCameraToAimDirection(playerView.ArsenalView.ActiveGun.AimRange, playerView.transform);
                }
                else
                {
                    ResetCamera();
                }
            }));
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        //метод смещает камеру в направлении прицеливания с помощью установления смещения Транспозера Синемашины

        public void FollowCameraToAimDirection(float aimingRange, Transform playerTransform)
        {
            _cinemachineFollow.FollowOffset = Vector3.Lerp(_cinemachineFollow.FollowOffset,
                playerTransform.forward * aimingRange, Time.deltaTime * 0.5f);
        }

        //метод возвращает камеру в центр игрока если игрок не прицеливается

        public void ResetCamera()
        {
            _cinemachineFollow.FollowOffset = Vector3.Lerp(_cinemachineFollow.FollowOffset,
                Vector3.zero, Time.deltaTime * _cameraSettings.FollowSpeed);
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