using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using Unity.Cinemachine;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View
{
    public class CameraBinder: MonoBehaviour
    {
        private CameraViewModel _cameraViewModel;
        private CinemachineCamera _camera;
        public void Bind(CameraViewModel cameraViewModel, HeroBinder hero)
        {
            _cameraViewModel = cameraViewModel;
            _camera = GetComponent<CinemachineCamera>();
            _camera.Follow = hero.transform;
            _cameraViewModel.SetCameraViewWithComponent(this, hero);
        }
    }
}