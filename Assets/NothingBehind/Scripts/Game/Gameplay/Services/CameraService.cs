using System;
using System.Collections;
using NothingBehind.Scripts.Game.Gameplay.Services.InputManager;
using NothingBehind.Scripts.Game.Gameplay.View;
using NothingBehind.Scripts.Utils;
using R3;
using Unity.Cinemachine;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class CameraService
    {
        private readonly GameplayInputManager _inputManager;
        private readonly Coroutines _coroutines;
        public ReadOnlyReactiveProperty<CameraViewModel> CameraViewModel => _cameraViewModel;

        private CameraBinder _cameraView;
        private CinemachineCamera _cinemachineCamera;
        private CinemachineFollow _cinemachineFollow;

        private readonly ReactiveProperty<CameraViewModel> _cameraViewModel = new();
        private readonly ReadOnlyReactiveProperty<bool> _isRotateCameraLeft;
        private readonly ReadOnlyReactiveProperty<bool> _isRotateCameraRight;

        private bool _progressRotate;
        private float _rotationSpeed = 60f;
        private float _followSpeed = 0.5f;

        public CameraService(GameplayInputManager inputManager, Coroutines coroutines)
        {
            _inputManager = inputManager;
            _coroutines = coroutines;
            CreateCameraViewModel();

            _isRotateCameraLeft = inputManager.IsRotateCameraLeft;
            _isRotateCameraRight = inputManager.IsRotateCameraRight;
            _isRotateCameraLeft.Skip(1).Subscribe(_=>coroutines.StartCoroutine(RotateBy45Deg(Vector3.up)));
            _isRotateCameraRight.Skip(1).Subscribe(_=>coroutines.StartCoroutine(RotateBy45Deg(Vector3.down)));
        }

        private void CreateCameraViewModel()
        {
            var cameraViewModel = new CameraViewModel(this);

            _cameraViewModel.OnNext(cameraViewModel);
        }

        public void BindCameraViewComponent(CameraBinder cameraView,
            CinemachineCamera cinemachineCamera,
            CinemachineFollow cinemachineFollow)
        {
            _cameraView = cameraView;
            _cinemachineCamera = cinemachineCamera;
            _cinemachineFollow = cinemachineFollow;
        }

        public IEnumerator RotateBy45Deg(Vector3 rotDir)
        {
            
            if (_progressRotate)
            {
                yield break;
            }

            var transform = _cameraView.transform;
            _progressRotate = true;
            float t = 0;
            Quaternion start = transform.rotation;
            Quaternion target = Quaternion.AngleAxis(45, rotDir) * transform.localRotation;

            do
            {
                if (transform==null)
                    yield break;
                
                t += _rotationSpeed / 45f * Time.deltaTime;

                transform.rotation = Quaternion.Lerp(start, target, t);
                yield return null;
            } while (t < 1f);

            _progressRotate = false;
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
                Vector3.zero, Time.deltaTime * _followSpeed);
        }

        //метод переводит камеру на ГГ когда к нему переходит управление 
        public void CameraFollow(Transform transform)
        {
            _cinemachineCamera.Follow = transform;
        }
    }
}