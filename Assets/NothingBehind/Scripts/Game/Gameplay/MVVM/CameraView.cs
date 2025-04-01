using NothingBehind.Scripts.Game.Gameplay.MVVM.Characters;
using Unity.Cinemachine;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM
{
    public class CameraView: MonoBehaviour
    {
        private CameraViewModel _cameraViewModel;
        private CinemachineCamera _camera;
        public void Bind(CameraViewModel cameraViewModel, PlayerView hero)
        {
            _cameraViewModel = cameraViewModel;
            _camera = GetComponent<CinemachineCamera>();
            _camera.Follow = hero.transform;
            _cameraViewModel.SetCameraViewWithComponent(this, hero);
        }
    }
}